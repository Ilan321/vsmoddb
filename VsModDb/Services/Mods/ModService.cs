using System.Net.Mime;
using VsModDb.Data;
using VsModDb.Data.Entities;
using VsModDb.Data.Entities.Assets;
using VsModDb.Data.Repositories;
using VsModDb.Models.Assets;
using VsModDb.Models.Exceptions;
using VsModDb.Services.Storage;
using VsModDb.Services.Storage.Providers;

namespace VsModDb.Services.Mods;

public interface IModService
{
    Task<AssetStream?> GetBannerAsync(int modId, CancellationToken cancellationToken = default);
    Task<bool> IsUserContributorAsync(int modId, User user, CancellationToken cancellationToken = default);
    Task SetBannerAsync(int modId, AssetStream stream, CancellationToken cancellationToken = default);
}

public class ModService(
    ILogger<ModService> log,
    IModRepository modRepository,
    IStorageProvider storageProvider
) : IModService
{
    public async Task<AssetStream?> GetBannerAsync(int modId, CancellationToken cancellationToken)
    {
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

    public Task<bool> IsUserContributorAsync(int modId, User user, CancellationToken cancellationToken = default)
    {
        // TODO: implement this

        return Task.FromResult(true);
    }

    public async Task SetBannerAsync(int modId, AssetStream stream, CancellationToken cancellationToken = default)
    {
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
}