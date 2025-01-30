using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using VsModDb.Data;
using VsModDb.Models.Mods;
using VsModDb.Models.Options;
using VsModDb.Models.Responses.Mods;
using VsModDb.Services.LegacyApi;
using VsModDb.Services.Mods;

namespace VsModDb.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/mods")]
public class ModsController(
    IOptions<LegacyClientOptions> legacyOptions,
    ILegacyApiClient legacyApiClient,
    ModDbContext context, 
    IModService modService
) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet]
    public async Task<GetModsResponse> GetMods(
        [FromQuery] ModSortType sort,
        [FromQuery] ModSortDirection direction,
        [FromQuery] int take,
        [FromQuery] int skip,
        [FromQuery] string? author,
        CancellationToken cancellationToken
    )
    {
        if (legacyOptions.Value.Enabled)
        {
            return await legacyApiClient.GetModsAsync(sort, direction, take, skip, author, cancellationToken);
        }

        throw new NotImplementedException();
    }

    [AllowAnonymous]
    [HttpGet("latest")]
    [ResponseCache(Duration = 300)]
    public async Task<List<ModDisplayDto>> GetLatestMods(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default
    )
    {
        if (legacyOptions.Value.Enabled)
        {
            return await legacyApiClient.GetLatestModsAsync(cancellationToken);
        }

        var mods = await context.Mods
            .AsNoTracking()
            .OrderByDescending(f => f.Id)
            .Take(count)
            .Select(f => new ModDisplayDto
            {
                Id = f.Id,
                UrlAlias = f.UrlAlias,
                Name = f.Name,
                Summary = f.Summary,
                Comments = f.Comments.Count
            })
            .ToListAsync(cancellationToken: cancellationToken);

        return mods;
    }

    [AllowAnonymous]
    [HttpGet("latest/comments")]
    public async Task<List<LatestModCommentDto>> GetLatestModComments(CancellationToken cancellationToken = default)
    {
        if (legacyOptions.Value.Enabled)
        {
            return await legacyApiClient.GetLatestModCommentsAsync(cancellationToken);
        }

        var comments = await context
            .ModComments
            .AsNoTracking()
            .OrderByDescending(f => f.TimeCreatedUtc)
            .Take(25)
            .Select(f => new LatestModCommentDto
            {
                Comment = new()
                {
                    Author = f.LinkedUser.UserName,
                    Comment = f.Comment,
                    TimeCreatedUtc = f.TimeCreatedUtc,
                    TimeUpdatedUtc = f.TimeUpdatedUtc,
                    ContentType = f.ContentType
                },
                Mod = new()
                {
                    Id = f.LinkedModId,
                    Name = f.LinkedMod.Name,
                    Comments = f.LinkedMod.Comments.Count,
                    Summary = f.LinkedMod.Summary,
                    UrlAlias = f.LinkedMod.UrlAlias,
                    Downloads = 0 // TODO: Implement download count
                }
            })
            .ToListAsync(cancellationToken: cancellationToken);

        return comments;
    }
}