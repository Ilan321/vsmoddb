using Microsoft.EntityFrameworkCore;
using VsModDb.Data.Entities.Assets;
using VsModDb.Data.Entities.Mods;
using VsModDb.Models.Assets;

namespace VsModDb.Data.Repositories;

public interface IModRepository
{
    Task<Mod?> FindModByIdAsync(int id, CancellationToken cancellationToken = default);

    Task UpdateModBannerAsync(
        int id,
        string assetPath,
        AssetStream assetStream,
        CancellationToken cancellationToken = default
    );

    Task<Asset?> FindModBannerByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Mod?> FindModByUrlAliasAsync(string urlAlias, CancellationToken cancellationToken = default);
    Task<int?> FindModIdByAliasAsync(string alias, CancellationToken cancellationToken = default);
    Task<List<ModComment>> GetCommentsByModIdAsync(int modId, CancellationToken cancellationToken = default);
}

public class ModRepository(
    ModDbContext context
) : IModRepository
{
    public Task<Mod?> FindModByIdAsync(int id, CancellationToken cancellationToken = default) => context
        .Mods
        .AsNoTracking()
        .Where(f => f.Id == id)
        .FirstOrDefaultAsync(cancellationToken);

    public Task<Asset?> FindModBannerByIdAsync(int id, CancellationToken cancellationToken = default) => context
        .Mods
        .AsNoTracking()
        .Where(f => f.Id == id)
        .Select(f => f.Banner)
        .FirstOrDefaultAsync(cancellationToken: cancellationToken);

    public Task<Mod?> FindModByUrlAliasAsync(string urlAlias, CancellationToken cancellationToken = default) => context
        .Mods
        .AsNoTracking()
        .Where(f => f.UrlAlias == urlAlias)
        .FirstOrDefaultAsync(cancellationToken: cancellationToken);

    public Task<int?> FindModIdByAliasAsync(string alias, CancellationToken cancellationToken = default) => context
        .Mods
        .Where(f => f.UrlAlias == alias)
        .Select(f => f.Id as int?)
        .FirstOrDefaultAsync(cancellationToken: cancellationToken);

    public async Task<List<ModComment>> GetCommentsByModIdAsync(int modId, CancellationToken cancellationToken = default)
    {
        var mod = await context
            .Mods
            .AsNoTracking()
            .Include(f => f.Comments)
            .ThenInclude(f => f.LinkedUser)
            .Where(f => f.Id == modId)
            .FirstAsync(cancellationToken: cancellationToken);

        return mod.Comments.ToList();
    }

    public async Task UpdateModBannerAsync(
        int id,
        string assetPath,
        AssetStream assetStream,
        CancellationToken cancellationToken = default
    )
    {
        var mod = await context
            .Mods
            .Include(f => f.Banner)
            .Where(f => f.Id == id)
            .SingleAsync(cancellationToken: cancellationToken);

        mod.Banner = new Asset
        {
            FileName = assetStream.FileName,
            ContentType = assetStream.ContentType,
            AssetPath = assetPath
        };

        await context.SaveChangesAsync(cancellationToken);
    }
}