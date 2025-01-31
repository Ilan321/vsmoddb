using System.Resources;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Routing.Constraints;
using Microsoft.Extensions.Caching.Memory;
using VsModDb.Extensions;
using VsModDb.Json;
using VsModDb.Models.Legacy;
using VsModDb.Models.Mods;
using VsModDb.Models.Responses.Mods;
using VsModDb.Services.Storage.Providers;

namespace VsModDb.Services.LegacyApi;

public interface ILegacyApiClient
{
    Task<Stream?> GetModLogoAsync(string alias, CancellationToken cancellationToken = default);
    Task<ModDetailsDto?> GetModAsync(string alias, CancellationToken cancellationToken = default);
    Task<List<ModCommentDto>?> GetModCommentsAsync(string alias, CancellationToken cancellationToken = default);
    Task<List<ModDisplayDto>> GetLatestModsAsync(CancellationToken cancellationToken = default);
    Task<List<LatestModCommentDto>> GetLatestModCommentsAsync(CancellationToken cancellationToken = default);

    Task<GetModsResponse> GetModsAsync(
        ModSortType sort,
        ModSortDirection direction,
        int take,
        int skip,
        string? author,
        CancellationToken cancellationToken = default
    );

    Task<List<ModDisplayDto>> GetModsByAuthorAsync(string author, CancellationToken cancellationToken = default);
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
            UrlAlias = legacyMod.UrlAlias,
            Tags = await ToModTagsAsync(legacyMod.Tags),
            Author = legacyMod.Author,
            Side = legacyMod.Side,
            Downloads = legacyMod.Downloads,
            Follows = legacyMod.Follows
        };
    }

    private async Task<List<ModTagDto>> ToModTagsAsync(string[] tagNames)
    {
        var legacyTags = await GetLegacyTagsAsync();

        return tagNames
            .Select(tag => legacyTags.FirstOrDefault(f => f.Name == tag))
            .OfType<LegacyTag>()
            .Select(legacyTag => new ModTagDto { Value = legacyTag.Name, Color = legacyTag.Color })
            .ToList();
    }

    private async Task<List<LegacyTag>> GetLegacyTagsAsync(CancellationToken cancellationToken = default)
    {
        var cacheKey = $"legacy.tags";

        if (!memoryCache.TryGetValue<List<LegacyTag>>(cacheKey, out var tags) || tags is null)
        {
            using var httpResponse = await httpClient.GetAsync("/api/tags", cancellationToken);

            httpResponse.EnsureSuccessStatusCode();

            var response =
                await httpResponse.Content.ReadFromJsonAsync<LegacyGetTagsResponse>(JsonOptions, cancellationToken);

            response.EnsureSuccessStatusCode();

            tags = response!.Tags;

            memoryCache.Set(cacheKey, tags, TimeSpan.FromMinutes(30));
        }

        return tags;
    }

    public async Task<List<ModCommentDto>?> GetModCommentsAsync(
        string alias,
        CancellationToken cancellationToken = default
    )
    {
        var cacheKey = $"legacy.comments.{alias}";

        if (!memoryCache.TryGetValue<List<ModCommentDto>>(cacheKey, out var comments) || comments is null)
        {
            var mod = await GetModInternalAsync(alias, cancellationToken);

            if (mod is null)
            {
                return null;
            }

            using var httpResponse = await httpClient.GetAsync($"api/comments/{mod.AssetId}", cancellationToken);

            httpResponse.EnsureSuccessStatusCode();

            var response =
                await httpResponse.Content.ReadFromJsonAsync<LegacyGetCommentsResponse>(JsonOptions, cancellationToken);

            if (response is null)
            {
                return null;
            }

            if (!response.IsSuccess)
            {
                return null;
            }

            comments = await response.Comments!.SelectAsync(ToModCommentAsync).ToListAsync(cancellationToken);

            memoryCache.Set(cacheKey, comments, TimeSpan.FromMinutes(5));
        }

        return comments;

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

            var response =
                await httpResponse.Content.ReadFromJsonAsync<LegacyGetModsResponse>(JsonOptions, cancellationToken);

            response.EnsureSuccessStatusCode();

            mods = response!.Mods
                .Take(10)
                .Select(ToModDisplayDto)
                .ToList();

            memoryCache.Set(cacheKey, mods, TimeSpan.FromMinutes(5));
        }

        return mods;
    }

    public async Task<List<LatestModCommentDto>> GetLatestModCommentsAsync(
        CancellationToken cancellationToken = default
    )
    {
        var cacheKey = "legacy.comments.latest";

        if (!memoryCache.TryGetValue<List<LatestModCommentDto>>(cacheKey, out var comments) || comments is null)
        {
            log.LogDebug("Fetching latest comments from moddb api");

            using var httpResponse = await httpClient.GetAsync("api/comments", cancellationToken);

            httpResponse.EnsureSuccessStatusCode();

            var response =
                await httpResponse.Content.ReadFromJsonAsync<LegacyGetCommentsResponse>(JsonOptions, cancellationToken);

            response.EnsureSuccessStatusCode();

            var mods = await GetModsAsync(cancellationToken);

            comments = await response!.Comments!
                .Take(20)
                .SelectAsync(async comment =>
                {
                    var userName = await GetUserAsync(comment.UserId, cancellationToken);
                    var modDetails = mods.FirstOrDefault(f => f.AssetId == comment.AssetId);

                    return new LatestModCommentDto
                    {
                        Mod = new()
                        {
                            Name = modDetails!.Name,
                            Id = modDetails.ModId,
                            Comments = modDetails.Comments,
                            Downloads = modDetails.Downloads,
                            Summary = modDetails.Summary,
                            UrlAlias = modDetails.UrlAlias
                        },
                        Comment = new()
                        {
                            Author = userName!,
                            Comment = comment.Text,
                            ContentType = ModCommentContentType.Html,
                            TimeCreatedUtc = comment.Created,
                            TimeUpdatedUtc = comment.LastModified
                        }
                    };
                }).ToListAsync(cancellationToken);

            memoryCache.Set(cacheKey, comments, TimeSpan.FromMinutes(5));
        }

        return comments;
    }

    public async Task<GetModsResponse> GetModsAsync(
        ModSortType sort,
        ModSortDirection direction,
        int take,
        int skip,
        string? author,
        CancellationToken cancellationToken = default
    )
    {
        var allMods = await GetModsAsync(cancellationToken);

        var query = allMods
            .AsEnumerable();

        if (!string.IsNullOrWhiteSpace(author))
        {
            query = query.Where(f => f.Author.Contains(author, StringComparison.OrdinalIgnoreCase));
        }

        if (direction == ModSortDirection.Ascending)
        {
            query = sort switch
            {
                ModSortType.Created => query.OrderBy(f => f.ModId), // TODO: fix this
                ModSortType.Downloads => query.OrderBy(f => f.Downloads),
                ModSortType.Comments => query.OrderBy(f => f.Comments),
                ModSortType.Trending => query.OrderBy(f => f.TrendingPoints),
                ModSortType.Updated => query.OrderBy(f => f.LastReleased),
                ModSortType.Name or _ => query.OrderBy(f => f.Name)
            };
        }
        else
        {
            query = sort switch
            {
                ModSortType.Created => query.OrderByDescending(f => f.ModId), // TODO: fix this
                ModSortType.Downloads => query.OrderByDescending(f => f.Downloads),
                ModSortType.Comments => query.OrderByDescending(f => f.Comments),
                ModSortType.Trending => query.OrderByDescending(f => f.TrendingPoints),
                ModSortType.Updated => query.OrderByDescending(f => f.LastReleased),
                ModSortType.Name or _ => query.OrderByDescending(f => f.Name)
            };
        }

        query = query.Skip(skip)
            .Take(take);

        var mods = query.Select(ToModDisplayDto).ToList();

        return new()
        {
            TotalMods = allMods.Count,
            Mods = mods
        };
    }

    public async Task<List<ModDisplayDto>> GetModsByAuthorAsync(string author, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"legacy.mods.by-author";

        if (!memoryCache.TryGetValue<List<ModDisplayDto>>(cacheKey, out var authorMods) || authorMods is null)
        {
            log.LogDebug("Fetching mods by author {author} from legacy api", author);

            var allMods = await GetModsAsync(cancellationToken);

            authorMods = allMods
                .Where(f => string.Equals(f.Author, author, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(f => f.ModId)
                .Select(ToModDisplayDto)
                .ToList();

            memoryCache.Set(cacheKey, authorMods, TimeSpan.FromMinutes(5));
        }

        return authorMods;
    }

    private async Task<LegacyModDetails?> GetModByAssetIdInternalAsync(
        int assetId,
        CancellationToken cancellationToken = default
    )
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

        var response =
            await httpResponse.Content.ReadFromJsonAsync<LegacyGetAuthorsResponse>(JsonOptions, cancellationToken);

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

            var response =
                await httpResponse.Content.ReadFromJsonAsync<LegacyGetModsResponse>(JsonOptions, cancellationToken);

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

    private async Task<LegacyModDetails?> GetModInternalAsync(
        string alias,
        CancellationToken cancellationToken = default
    )
    {
        var cacheKey = $"legacy.mods.{alias}";

        if (memoryCache.TryGetValue<LegacyModDetails>(cacheKey, out var cachedMod) && cachedMod is not null)
        {
            return cachedMod;
        }

        var modId = await GetModIdAsync(alias, cancellationToken);

        if (modId is null)
        {
            return null;
        }

        var httpResponse = await httpClient.GetAsync($"api/mod/{modId}", cancellationToken);

        httpResponse.EnsureSuccessStatusCode();

        var response =
            await httpResponse.Content.ReadFromJsonAsync<LegacyGetModDetailsResponse>(JsonOptions, cancellationToken);

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

    private async Task<int?> GetModIdAsync(string alias, CancellationToken cancellationToken)
    {
        var mods = await GetModsAsync(cancellationToken);

        var cacheKey = $"legacy.alias-to-id.{alias}";

        if (!memoryCache.TryGetValue<int?>(cacheKey, out var id) || !id.HasValue)
        {
            if (!int.TryParse(alias, out var parsedId))
            {
                var modDetails = mods.FirstOrDefault(f => f.UrlAlias == alias);

                id = modDetails?.ModId;
            }

            var mod = mods.FirstOrDefault(f => f.ModId == parsedId);

            if (mod is not null)
            {
                id = mod.ModId;
            }
        }

        return id;
    }

    private static ModDisplayDto ToModDisplayDto(LegacyMod f) => new()
    {
        Name = f.Name,
        Id = f.ModId,
        Comments = f.Comments,
        Downloads = f.Downloads,
        Summary = f.Summary,
        UrlAlias = f.UrlAlias
    };
}