using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Mime;
using System.Text.Json;
using System.Text.Json.Serialization;
using Ilan321.AspNetCore.Caching.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using VsModDb.Extensions;
using VsModDb.Json;
using VsModDb.Models.Assets;
using VsModDb.Models.Legacy;
using VsModDb.Models.Mods;
using VsModDb.Models.Requests.Mods;
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

    Task<AssetStream?> GetModFileAsync(
        string alias,
        string version,
        CancellationToken cancellationToken = default
    );

    Task<GetModsResponse> SearchModsAsync(SearchModsRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Queries all mods' details and populates the cache with the results.
    /// </summary>
    Task HydrateModDetailsAsync(CancellationToken cancellationToken = default);
}

public class LegacyApiClient(
    ILogger<LegacyApiClient> log,
    HttpClient httpClient,
    IMemoryCache memoryCache,
    IDistributedCache distributedCache,
    IStorageProvider storageProvider
) : ILegacyApiClient
{
    private const string AllModDetailsCacheKey = "legacy.mods.all-details";

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
            Follows = legacyMod.Follows,
            Releases = ToModReleaseDtos(legacyMod.Releases)
        };
    }

    private async Task<List<LegacyTag>> GetLegacyTagsAsync(CancellationToken cancellationToken = default)
    {
        var response = await GetOrFetchAsync<LegacyGetTagsResponse>(
            "legacy.tags",
            "api/tags",
            TimeSpan.FromMinutes(30),
            cancellationToken
        );

        return response!.Tags;
    }

    public async Task<List<ModCommentDto>?> GetModCommentsAsync(
        string alias,
        CancellationToken cancellationToken = default
    )
    {
        var mod = await GetModInternalAsync(alias, cancellationToken);

        var response = await GetOrFetchAsync<LegacyGetCommentsResponse>(
            $"legacy.comments.by-mod.{mod.ModId}",
            $"api/comments/{mod.AssetId}",
            TimeSpan.FromMinutes(5),
            cancellationToken
        );

        return await response!.Comments!.SelectAsync(ToModCommentAsync).ToListAsync(cancellationToken);

        async Task<ModCommentDto> ToModCommentAsync(LegacyComment comment)
        {
            var userName = await GetUserAsync(comment.UserId, cancellationToken);

            return new ModCommentDto
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
        var response = await GetOrFetchAsync<LegacyGetModsResponse>(
            "legacy.mods.latest",
            "api/mods?orderby=asset.created",
            TimeSpan.FromMinutes(5),
            cancellationToken
        );

        return response!.Mods
            .Take(10)
            .Select(ToModDisplayDto)
            .ToList();
    }

    public async Task<List<LatestModCommentDto>> GetLatestModCommentsAsync(
        CancellationToken cancellationToken = default
    )
    {
        var response = await GetOrFetchAsync<LegacyGetCommentsResponse>(
            "legacy.comments.latest",
            "api/comments",
            TimeSpan.FromMinutes(5),
            cancellationToken
        );

        var mods = await GetModsAsync(cancellationToken);

        return await response!.Comments!
            .Take(20)
            .SelectAsync(
                async comment =>
                {
                    var userName = await GetUserAsync(comment.UserId, cancellationToken);
                    var modDetails = mods.FirstOrDefault(f => f.AssetId == comment.AssetId);

                    return new LatestModCommentDto
                    {
                        Mod = ToModDisplayDto(modDetails!),
                        Comment = new()
                        {
                            Author = userName!,
                            Comment = comment.Text,
                            ContentType = ModCommentContentType.Html,
                            TimeCreatedUtc = comment.Created,
                            TimeUpdatedUtc = comment.LastModified
                        }
                    };
                }
            ).ToListAsync(cancellationToken);
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

    public async Task<List<ModDisplayDto>> GetModsByAuthorAsync(
        string author,
        CancellationToken cancellationToken = default
    )
    {
        var allMods = await GetModsAsync(cancellationToken);

        return allMods
            .Where(
                f => string.Equals(
                    f.Author,
                    author,
                    StringComparison.OrdinalIgnoreCase
                )
            )
            .OrderByDescending(f => f.ModId)
            .Select(ToModDisplayDto)
            .ToList();
    }

    public async Task<AssetStream?> GetModFileAsync(
        string alias,
        string version,
        CancellationToken cancellationToken = default
    )
    {
        var mod = await GetModInternalAsync(alias, cancellationToken);

        if (mod is null)
        {
            return null;
        }

        var release = mod.Releases.FirstOrDefault(f => f.ModVersion == version);

        if (release is null)
        {
            return null;
        }

        using var httpResponse = await httpClient.GetAsync(release.MainFile, cancellationToken);

        httpResponse.EnsureSuccessStatusCode();

        // TODO: obviously this isn't great

        var ms = new MemoryStream();

        await httpResponse.Content.CopyToAsync(ms, cancellationToken);

        ms.Seek(0, SeekOrigin.Begin);

        return new(
            ms,
            release.FileName,
            MediaTypeNames.Application.Zip
        );
    }

    public async Task<GetModsResponse> SearchModsAsync(
        SearchModsRequest request,
        CancellationToken cancellationToken = default
    )
    {
        const char ExactSearchChar = '"';

        var allMods = await GetModsAsync(cancellationToken);

        log.LogDebug("Performing search for mods using query {@query}", request);

        var query = allMods
            .AsEnumerable();

        if (!string.IsNullOrWhiteSpace(request.Text))
        {
            var isExactSearch = request.Text.StartsWith(ExactSearchChar) && request.Text.EndsWith(ExactSearchChar) &&
                                request.Text.Length > 2;

            var searchTerm = isExactSearch
                ? request.Text![1..^1]
                : request.Text!;

            query = query.Where(
                f =>
                    DoStringSearch(
                        f.Name,
                        searchTerm,
                        isExactSearch
                    ) ||
                    DoStringSearch(
                        f.Summary,
                        searchTerm,
                        isExactSearch
                    ) ||
                    DoStringSearch(
                        f.UrlAlias,
                        searchTerm,
                        isExactSearch
                    ) ||
                    DoStringSearch(
                        f.Author,
                        searchTerm,
                        isExactSearch
                    )
            );
        }

        // Check the cached mod details for the side/game version properties
        // If not found in cache, skip this filter

        if (!string.IsNullOrWhiteSpace(request.Side) || request.GameVersions is { Length: > 0 })
        {
            query = await FilterByModDetailsAsync(query, request);
        }

        if (request.Tags is { Count: > 0 })
        {
            query = query.Where(f => request.Tags.All(t => f.Tags.Contains(t, StringComparer.OrdinalIgnoreCase)));
        }

        if (request.Sort.HasValue)
        {
            var isDesc = request.Direction == ModSortDirection.Descending;

            query = request.Sort switch
            {
                ModSortType.Created => OrderBy(
                    query,
                    f => f.ModId,
                    isDesc
                ),
                ModSortType.Downloads => OrderBy(
                    query,
                    f => f.Downloads,
                    isDesc
                ),
                ModSortType.Comments => OrderBy(
                    query,
                    f => f.Comments,
                    isDesc
                ),
                ModSortType.Trending => OrderBy(
                    query,
                    f => f.TrendingPoints,
                    isDesc
                ),
                ModSortType.Name => OrderBy(
                    query,
                    f => f.Name,
                    isDesc
                ),
                ModSortType.Updated => OrderBy(
                    query,
                    f => f.LastReleased,
                    isDesc
                ),
                _ => query // Do nothing
            };
        }

        var mods = query.ToList();

        var page = mods
            .Skip(request.Skip ?? 0)
            .Take(request.Take ?? 25)
            .ToList();

        return new()
        {
            TotalMods = mods.Count,
            Mods = page.Select(ToModDisplayDto).ToList()
        };

        async Task<IEnumerable<LegacyMod>> FilterByModDetailsAsync(
            IEnumerable<LegacyMod> mods,
            SearchModsRequest request
        )
        {
            var modDetails = await TryGetCachedModDetails(cancellationToken);

            if (modDetails is null)
            {
                return mods.Where(_ => false);
            }

            return mods.Where(
                mod =>
                {
                    if (!modDetails.TryGetValue(mod.ModId, out var details))
                    {
                        return false;
                    }

                    // Filter by side if specified

                    if (!string.IsNullOrWhiteSpace(request.Side) && request.Side != "any")
                    {
                        if (details.Side != request.Side)
                        {
                            return false;
                        }
                    }

                    // Filter by game version if specified, if at least one version matches

                    if (request.GameVersions is { Length: > 0 })
                    {
                        if (!details.Releases.Any(f => request.GameVersions.Any(f.Tags.Contains)))
                        {
                            return false;
                        }
                    }

                    return true;
                }
            );
        }

        IOrderedEnumerable<TSource> OrderBy<TSource, TKey>(
            IEnumerable<TSource> range,
            Func<TSource, TKey> selector,
            bool isDesc
        ) => isDesc
            ? range.OrderByDescending(selector)
            : range.OrderBy(selector);
    }

    /// <summary>
    /// Queries all mods' details and populates the cache with the results.
    /// </summary>
    public async Task HydrateModDetailsAsync(CancellationToken cancellationToken = default)
    {
        log.LogDebug("Repopulating all mod details");

        var sw = new Stopwatch();

        sw.Start();

        var mods = await GetModsAsync(cancellationToken);

        log.LogDebug("Found {count} mods to repopulate details for", mods.Count);

        var dict = new ConcurrentDictionary<int, LegacyModDetails>();

        await Parallel.ForEachAsync(
            mods,
            cancellationToken,
            async (mod, _) =>
            {
                var modDetails = await GetModInternalAsync(mod.ModId, cancellationToken);

                if (modDetails == null)
                {
                    return;
                }

                dict.TryAdd(modDetails.ModId, modDetails);
            }
        );

        sw.Stop();

        log.LogDebug(
            "Finished fetching {count} mod details in {elapsed}, total mod details retrieved: {count}",
            mods.Count,
            sw.Elapsed,
            dict.Count
        );

        await distributedCache.SetAsync(
            AllModDetailsCacheKey,
            dict.ToDictionary(f => f.Key, f => f.Value),
            new DistributedCacheEntryOptions(),
            JsonExtensions.Default,
            cancellationToken
        );
    }

    public Task<Dictionary<int, LegacyModDetails>?> TryGetCachedModDetails(
        CancellationToken cancellationToken = default
    ) =>
        distributedCache.GetAsync<Dictionary<int, LegacyModDetails>?>(
            AllModDetailsCacheKey,
            JsonExtensions.Default,
            cancellationToken: cancellationToken
        );

    private async Task<LegacyModDetails?> GetModByAssetIdInternalAsync(
        int assetId,
        CancellationToken cancellationToken = default
    )
    {
        var mod = await GetOrFetchAsync<LegacyMod>(
            $"legacy.mods.byAssetId.{assetId}",
            async _ =>
            {
                var mods = await GetModsAsync(cancellationToken);

                return mods.FirstOrDefault(f => f.AssetId == assetId);
            },
            TimeSpan.FromMinutes(30), // asset id -> mod id should not change
            cancellationToken
        );

        if (mod is null)
        {
            return null;
        }

        return await GetModInternalAsync(mod.ModId, cancellationToken);
    }

    private async Task<Dictionary<int, string>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var response = await GetOrFetchAsync<LegacyGetAuthorsResponse>(
            "legacy.users",
            "api/authors",
            TimeSpan.FromMinutes(30),
            cancellationToken
        );

        return response!
            .Authors!
            .ToDictionary(f => f.UserId, f => f.Name);
    }

    private async Task<List<LegacyMod>> GetModsAsync(CancellationToken cancellationToken = default)
    {
        var response = await GetOrFetchAsync<LegacyGetModsResponse>(
            $"legacy.mods",
            "api/mods",
            TimeSpan.FromMinutes(5),
            cancellationToken
        );

        return response!.Mods;
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

            memoryCache.Set(
                RefetchCacheKey,
                true,
                TimeSpan.FromMinutes(5)
            );

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

            await storageProvider.SaveFileAsync(
                assetPath,
                logoStream,
                cancellationToken
            );
        }

        return await storageProvider.GetFileAsync(assetPath, cancellationToken);
    }

    private Task<LegacyModDetails?> GetModInternalAsync(int modId, CancellationToken cancellationToken = default) =>
        GetOrFetchAsync<LegacyModDetails?>(
            $"legacy.mods.by-id.{modId}",
            async _ =>
            {
                var response = await GetOrFetchAsync<LegacyGetModDetailsResponse>(
                    $"legacy.responses.mod-by-id.{modId}",
                    $"api/mod/{modId}",
                    TimeSpan.FromMinutes(5),
                    cancellationToken
                );

                return response?.Mod;
            },
            TimeSpan.FromMinutes(5),
            cancellationToken
        );

    private async Task<LegacyModDetails?> GetModInternalAsync(
        string alias,
        CancellationToken cancellationToken = default
    )
    {
        var modId = await GetModIdAsync(alias, cancellationToken);

        if (!modId.HasValue)
        {
            return null;
        }

        return await GetModInternalAsync(modId.Value, cancellationToken);
    }

    private Task<int?> GetModIdAsync(string alias, CancellationToken cancellationToken) =>
        GetOrFetchAsync<int?>(
            $"legacy.alias-to-id.{alias}",
            async ct =>
            {
                var mods = await GetModsAsync(cancellationToken);

                if (!int.TryParse(alias, out var parsedId))
                {
                    var modDetails = mods.FirstOrDefault(f => f.UrlAlias == alias);

                    return modDetails?.ModId;
                }

                var mod = mods.FirstOrDefault(f => f.ModId == parsedId);

                return mod?.ModId;
            },
            TimeSpan.FromMinutes(30),
            cancellationToken
        );

    private Task<T?> GetOrFetchAsync<T>(
        string cacheKey,
        string endpoint,
        TimeSpan cacheTtl,
        CancellationToken cancellationToken = default
    ) => GetOrFetchAsync<T>(
        cacheKey,
        async (ct) =>
        {
            using var httpResponse = await httpClient.GetAsync(endpoint, ct);

            httpResponse.EnsureSuccessStatusCode();

            var response = await httpResponse.Content.ReadFromJsonAsync<T>(JsonOptions, cancellationToken);

            if (response is BaseLegacyResponse baseResponse)
            {
                baseResponse.EnsureSuccessStatusCode();
            }

            return response;
        },
        cacheTtl,
        cancellationToken
    );

    private async Task<T?> GetOrFetchAsync<T>(
        string cacheKey,
        Func<CancellationToken, Task<T?>> fetch,
        TimeSpan cacheTtl,
        CancellationToken cancellationToken = default
    )
    {
        if (memoryCache.TryGetValue<T>(cacheKey, out var cachedValue) && cachedValue is not null)
        {
            return cachedValue;
        }

        var response = await fetch(cancellationToken);

        if (response is not null)
        {
            memoryCache.Set(
                cacheKey,
                response,
                cacheTtl
            );
        }

        return response;
    }


    private bool DoStringSearch(
        string? a,
        string b,
        bool isExact
    )
    {
        if (!isExact)
        {
            return a?.Contains(b, StringComparison.OrdinalIgnoreCase) == true;
        }

        if (string.IsNullOrWhiteSpace(a))
        {
            return false;
        }

        if (string.Equals(
                a,
                b,
                StringComparison.OrdinalIgnoreCase
            ))
        {
            return true;
        }

        // Try checking word by word

        var split = a.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        return split.Any(
            f => string.Equals(
                f,
                b,
                StringComparison.OrdinalIgnoreCase
            )
        );
    }

    private List<ModReleaseDto> ToModReleaseDtos(List<LegacyModRelease> legacyModReleases)
    {
        return legacyModReleases.Select(ToModReleaseDto).ToList();
    }

    private ModReleaseDto ToModReleaseDto(LegacyModRelease r) => new()
    {
        ModId = r.ModIdStr,
        ModVersion = r.ModVersion,
        TimeCreatedUtc = r.Created,
        Downloads = r.Downloads,
        FileName = r.FileName,
        GameVersions = r.Tags
    };

    private async Task<List<ModTagDto>> ToModTagsAsync(string[] tagNames)
    {
        var legacyTags = await GetLegacyTagsAsync();

        return tagNames
            .Select(tag => legacyTags.FirstOrDefault(f => f.Name == tag))
            .OfType<LegacyTag>()
            .Select(legacyTag => new ModTagDto { Value = legacyTag.Name, Color = legacyTag.Color })
            .ToList();
    }

    private static ModDisplayDto ToModDisplayDto(LegacyMod f) => new()
    {
        Name = f.Name,
        Id = f.ModId,
        Comments = f.Comments,
        Downloads = f.Downloads,
        Summary = f.Summary,
        UrlAlias = f.UrlAlias,
        Author = f.Author
    };
}