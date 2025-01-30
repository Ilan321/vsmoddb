using System.Net.Mime;
using VsModDb.Data;
using VsModDb.Data.Entities;
using VsModDb.Data.Entities.Assets;
using VsModDb.Data.Entities.Mods;
using VsModDb.Data.Repositories;
using VsModDb.Models.Assets;
using VsModDb.Models.Exceptions;
using VsModDb.Models.Mods;
using VsModDb.Services.Storage;
using VsModDb.Services.Storage.Providers;

namespace VsModDb.Services.Mods;

public interface IModService
{
    Task<AssetStream?> GetBannerAsync(string modAlias, CancellationToken cancellationToken = default);

    Task<bool> IsUserContributorAsync(
        string modAlias,
        User user,
        CancellationToken cancellationToken = default
    );

    Task SetBannerAsync(
        string modAlias,
        AssetStream stream,
        CancellationToken cancellationToken = default
    );

    Task<ModDetailsDto?> GetModDetailsAsync(string urlAlias, CancellationToken cancellationToken = default);
    Task<List<ModCommentDto>> GetModCommentsAsync(string alias, CancellationToken cancellationToken = default);
}

public class ModService(
    ILogger<ModService> log,
    IModRepository modRepository,
    IStorageProvider storageProvider
) : IModService
{
    public async Task<AssetStream?> GetBannerAsync(string modAlias, CancellationToken cancellationToken)
    {
        var modId = await GetModIdByAliasAsync(modAlias, cancellationToken);

        log.LogDebug("Getting mod banner for mod {modId}", modId);

        var modBanner = await modRepository.FindModBannerByIdAsync(modId, cancellationToken);

        if (modBanner is null)
        {
            return await GetDefaultModBannerAsync(cancellationToken);
        }

        var bannerStream = await GetModAssetAsync(modBanner, cancellationToken);

        if (bannerStream is null)
        {
            return await GetDefaultModBannerAsync(cancellationToken);
        }

        return bannerStream;
    }

    public Task<bool> IsUserContributorAsync(
        string modAlias,
        User user,
        CancellationToken cancellationToken = default
    )
    {
        // TODO: implement this

        return Task.FromResult(true);
    }

    public async Task SetBannerAsync(
        string modAlias,
        AssetStream stream,
        CancellationToken cancellationToken = default
    )
    {
        var modId = await GetModIdByAliasAsync(modAlias, cancellationToken);

        log.LogDebug("Updating mod banner for mod {modId}", modId);

        var mod = await modRepository.FindModByIdAsync(modId, cancellationToken);

        if (mod is null)
        {
            throw new ModNotFoundException(modId);
        }

        var assetPath = $"mods/{modId}/banner.png";

        await storageProvider.SaveFileAsync(assetPath, stream.Stream, cancellationToken);

        await modRepository.UpdateModBannerAsync(modId, assetPath, stream, cancellationToken);
    }

    public async Task<ModDetailsDto?> GetModDetailsAsync(string urlAlias, CancellationToken cancellationToken = default)
    {
        var mod = await modRepository.FindModByUrlAliasAsync(urlAlias, cancellationToken);

        return ToModDetailsAsync(mod, cancellationToken);
    }

    public async Task<List<ModCommentDto>> GetModCommentsAsync(
        string alias,
        CancellationToken cancellationToken = default
    )
    {
        var modId = await GetModIdByAliasAsync(alias, cancellationToken);

        var comments = await modRepository.GetCommentsByModIdAsync(modId, cancellationToken);

        return comments
            .Select(f => new ModCommentDto
            {
                Author = f.LinkedUser.UserName!,
                Comment = f.Comment,
                TimeCreatedUtc = f.TimeCreatedUtc,
                TimeUpdatedUtc = f.TimeUpdatedUtc,
                ContentType = f.ContentType
            })
            .ToList();
    }

    private ModDetailsDto? ToModDetailsAsync(Mod? mod, CancellationToken cancellationToken = default)
    {
        if (mod is null)
        {
            return null;
        }

        return new ModDetailsDto
        {
            Id = mod.Id,
            Name = mod.Name,
            Summary = mod.Summary,
            TimeCreatedUtc = mod.TimeCreatedUtc,
            TimeUpdatedUtc = mod.TimeUpdatedUtc,
            Description = mod.Description,
            UrlAlias = mod.UrlAlias
        };
    }

    private async Task<AssetStream?> GetModAssetAsync(Asset asset, CancellationToken cancellationToken)
    {
        var stream = await storageProvider.GetFileAsync(asset.AssetPath, cancellationToken);

        return new AssetStream(stream, asset.FileName, asset.ContentType);
    }

    private async Task<AssetStream> GetDefaultModBannerAsync(CancellationToken cancellationToken)
    {
        const string DefaultModBannerPath = "mods/mod-default.png";

        var stream = await storageProvider.GetFileAsync(DefaultModBannerPath, cancellationToken);

        return new AssetStream(stream, "mod-default.png", MediaTypeNames.Image.Png);
    }

    private async Task<int> GetModIdByAliasAsync(string alias, CancellationToken cancellationToken)
    {
        if (int.TryParse(alias, out var parsedId))
        {
            return parsedId;
        }

        var modId = await modRepository.FindModIdByAliasAsync(alias, cancellationToken);

        return modId ?? throw new ModNotFoundException(alias);
    }
}