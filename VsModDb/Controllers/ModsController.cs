using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VsModDb.Data;
using VsModDb.Models.Mods;
using VsModDb.Services.Mods;

namespace VsModDb.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/mods")]
public class ModsController(ModDbContext context, IModService modService) : ControllerBase
{
    [HttpGet("latest")]
    [ResponseCache(Duration = 300)]
    public async Task<List<ModDisplayDto>> GetLatestMods(
        [FromQuery] int count = 10,
        CancellationToken cancellationToken = default
    )
    {
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