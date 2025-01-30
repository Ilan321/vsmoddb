using System.Resources;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Caching.Memory;
using VsModDb.Extensions;
using VsModDb.Json;
using VsModDb.Models.Legacy;
using VsModDb.Models.Mods;
using VsModDb.Services.Storage.Providers;

namespace VsModDb.Services.LegacyApi;

public interface ILegacyApiClient
{
    Task<Stream?> GetModLogoAsync(string alias, CancellationToken cancellationToken = default);
    Task<ModDetailsDto?> GetModAsync(string alias, CancellationToken cancellationToken = default);
    Task<List<ModCommentDto>?> GetModCommentsAsync(string alias, CancellationToken cancellationToken = default);
    Task<List<ModDisplayDto>> GetLatestModsAsync(CancellationToken cancellationToken = default);
    Task<List<LatestModCommentDto>> GetLatestModCommentsAsync(CancellationToken cancellationToken = default);
}

public class LegacyApiClient(
    ILogger<LegacyApiClient> log,
    HttpClient httpClient,
    IMemoryCache memoryCache,
    IStorageProvider storageProvider
) : ILegacyApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = ConfigureJsonOptions();

    public async Task<ModDetailsDto?> GetModAsync(string alias, CancellationToken cancellationToken = default)
    {
        var legacyMod = await GetModInternalAsync(alias, cancellationToken);

        if (legacyMod is null)
        {
            return null;
        }

        return new()
        {
            Name = legacyMod.Name,
            TimeCreatedUtc = legacyMod.Created,
            TimeUpdatedUtc = legacyMod.LastModified,
            Summary = "test summary", // TODO: fix this
            Description = legacyMod.Text,
            Id = legacyMod.ModId,
            UrlAlias = legacyMod.UrlAlias
        };
    }

    public async Task<List<ModCommentDto>?> GetModCommentsAsync(string alias, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"legacy.comments.{alias}";

        if (memoryCache.TryGetValue<List<ModCommentDto>>(cacheKey, out var modComments) && modComments is not null)
        {
            return modComments;
        }

        using var httpResponse = await httpClient.GetAsync($"api/comments/{alias}", cancellationToken);

        httpResponse.EnsureSuccessStatusCode();

        var response = await httpResponse.Content.ReadFromJsonAsync<LegacyGetCommentsResponse>(JsonOptions, cancellationToken);

        if (response is null)
        {
            return null;
        }

        if (!response.IsSuccess)
        {
            return null;
        }

        return await response.Comments!.SelectAsync(ToModCommentAsync).ToListAsync(cancellationToken);

        async Task<ModCommentDto> ToModCommentAsync(LegacyComment comment)
        {
            var userName = await GetUserAsync(comment.UserId, cancellationToken);

            return new()
            {
                Author = userName ?? "unknown user",
                Comment = comment.Text,
                TimeCreatedUtc = comment.Created,
                TimeUpdatedUtc = comment.LastModified,
                ContentType = ModCommentContentType.Html // Legacy moddb uses HTML only
            };
        }
    }

    public async Task<List<ModDisplayDto>> GetLatestModsAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = "legacy.mods.latest";

        if (!memoryCache.TryGetValue<List<ModDisplayDto>>(cacheKey, out var mods) || mods is null)
        {
            log.LogDebug("Fetching latest mods from moddb api");

            using var httpResponse = await httpClient.GetAsync("api/mods?orderby=asset.created", cancellationToken);

            httpResponse.EnsureSuccessStatusCode();

            var response = await httpResponse.Content.ReadFromJsonAsync<LegacyGetModsResponse>(JsonOptions, cancellationToken);

            response.EnsureSuccessStatusCode();

            mods = response!.Mods
                .OrderByDescending(f => f.LastReleased)
                .Take(10)
                .Select(f => new ModDisplayDto
                {
                    Name = f.Name,
                    Id = f.ModId,
                    Comments = f.Comments,
                    Downloads = f.Downloads,
                    Summary = f.Summary,
                    UrlAlias = f.UrlAlias
                })
                .ToList();

            memoryCache.Set(cacheKey, mods, TimeSpan.FromMinutes(5));
        }

        return mods;
    }

    public async Task<List<LatestModCommentDto>> GetLatestModCommentsAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = "legacy.comments.latest";

        if (memoryCache.TryGetValue<List<LatestModCommentDto>>(cacheKey, out var comments) && comments is not null)
        {
            return comments;
        }

        log.LogDebug("Fetching latest comments from moddb api");

        using var httpResponse = await httpClient.GetAsync("api/comments", cancellationToken);

        httpResponse.EnsureSuccessStatusCode();

        var response = await httpResponse.Content.ReadFromJsonAsync<LegacyGetCommentsResponse>(JsonOptions, cancellationToken);

        response.EnsureSuccessStatusCode();

        var mods = await GetModsAsync(cancellationToken);

        comments = await response!.Comments!.SelectAsync(ToLatestModCommentAsync).ToListAsync(cancellationToken);

        return comments;


        async Task<LatestModCommentDto> ToLatestModCommentAsync(LegacyComment comment)
        {
            var userName = await GetUserAsync(comment.UserId, cancellationToken);
            var modDetails = mods.FirstOrDefault(f => f.AssetId == comment.AssetId);


            return new()
            {
                Mod = new()
                {
                    Name = modDetails.Name,
                    Id = modDetails.ModId,
                    Comments = modDetails.Comments,
                    Downloads = modDetails.Downloads,
                    Summary = modDetails.Summary,
                    UrlAlias = modDetails.UrlAlias
                },
                Comment = new()
                {
                    Author = userName,
                    Comment = comment.Text,
                    ContentType = ModCommentContentType.Html,
                    TimeCreatedUtc = comment.Created,
                    TimeUpdatedUtc = comment.LastModified
                }
            };
        }
    }

    private async Task<LegacyModDetails?> GetModByAssetIdInternalAsync(int assetId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"legacy.mods.byAssetId.{assetId}";

        if (memoryCache.TryGetValue<LegacyModDetails>(cacheKey, out var cachedMod) && cachedMod is not null)
        {
            return cachedMod;
        }

        var mods = await GetModsAsync(cancellationToken);

        var mod = mods.FirstOrDefault(f => f.AssetId == assetId);

        if (mod is null)
        {
            return null;
        }

        return await GetModInternalAsync(mod.ModId.ToString(), cancellationToken);
    }

    private async Task<Dictionary<int, string>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = "legacy.users";

        if (memoryCache.TryGetValue<Dictionary<int, string>>(cacheKey, out var userMap) && userMap is not null)
        {
            return userMap;
        }

        log.LogDebug("Fetching authors from moddb api");

        using var httpResponse = await httpClient.GetAsync("api/authors", cancellationToken);

        var response = await httpResponse.Content.ReadFromJsonAsync<LegacyGetAuthorsResponse>(JsonOptions, cancellationToken);

        if (response?.IsSuccess != true)
        {
            throw new HttpRequestException(
                $"Non-success status code returned from Authors endpoint: {response?.StatusCode}");
        }

        userMap = response!.Authors!.ToDictionary(f => f.UserId, f => f.Name);

        memoryCache.Set(cacheKey, userMap, TimeSpan.FromMinutes(30));

        return userMap;
    }

    private async Task<List<LegacyMod>> GetModsAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = $"legacy.mods";

        if (!memoryCache.TryGetValue<List<LegacyMod>>(cacheKey, out var mods) || mods is null)
        {
            using var httpResponse = await httpClient.GetAsync("api/mods", cancellationToken);

            httpResponse.EnsureSuccessStatusCode();

            var response = await httpResponse.Content.ReadFromJsonAsync<LegacyGetModsResponse>(JsonOptions, cancellationToken);

            response.EnsureSuccessStatusCode();

            mods = response!.Mods;

            memoryCache.Set(cacheKey, mods, TimeSpan.FromMinutes(5));
        }

        return mods;
    }

    private async Task<string?> GetUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        const string RefetchCacheKey = "legacy.users.refetch";

        var map = await GetUsersAsync(cancellationToken);

        if (!map.TryGetValue(userId, out var userName))
        {
            if (memoryCache.TryGetValue<bool>(RefetchCacheKey, out var alreadyRefetched) && alreadyRefetched)
            {
                return null;
            }

            log.LogDebug("Could not find user {userName} in authors map, clearing cache and trying again", userId);

            memoryCache.Remove("legacy.users");

            memoryCache.Set(RefetchCacheKey, true, TimeSpan.FromMinutes(5));

            map = await GetUsersAsync(cancellationToken);

            userName = map.GetValueOrDefault(userId);
        }

        return userName;
    }

    private static JsonSerializerOptions ConfigureJsonOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = new LowerCaseNamingPolicy(),
            Converters =
            {
                new JsonStringEnumConverter(allowIntegerValues: false)
            }
        };

        return options;
    }

    public async Task<Stream?> GetModLogoAsync(string alias, CancellationToken cancellationToken = default)
    {
        var assetPath = $"legacy/{alias}/logo.png";

        if (!await storageProvider.DoesFileExistAsync(assetPath, cancellationToken))
        {
            log.LogDebug("Fetching legacy mod logo for mod {alias}", alias);

            var modDetails = await GetModInternalAsync(alias, cancellationToken);

            if (modDetails is null)
            {
                return null;
            }

            using var logoResponse = string.IsNullOrWhiteSpace(modDetails.LogoFileName)
                ? await httpClient.GetAsync("/web/img/mod-default.png", cancellationToken)
                : await httpClient.GetAsync(modDetails.LogoFileName, cancellationToken);

            logoResponse.EnsureSuccessStatusCode();

            await using var logoStream = await logoResponse.Content.ReadAsStreamAsync(cancellationToken);

            await storageProvider.SaveFileAsync(assetPath, logoStream, cancellationToken);
        }

        return await storageProvider.GetFileAsync(assetPath, cancellationToken);
    }

    private async Task<LegacyModDetails?> GetModInternalAsync(string alias, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"legacy.mods.{alias}";

        if (memoryCache.TryGetValue<LegacyModDetails>(cacheKey, out var cachedMod) && cachedMod is not null)
        {
            return cachedMod;
        }

        var httpResponse = await httpClient.GetAsync($"api/mod/{alias}", cancellationToken);

        httpResponse.EnsureSuccessStatusCode();

        var response = await httpResponse.Content.ReadFromJsonAsync<LegacyGetModDetailsResponse>(JsonOptions, cancellationToken);

        if (response is null)
        {
            return null;
        }

        if (!response.IsSuccess)
        {
            return null;
        }

        var modDetails = response.Mod!;

        memoryCache.Set(cacheKey, modDetails, TimeSpan.FromMinutes(5));

        return modDetails;
    }
}