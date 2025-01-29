using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VsModDb.Data;
using VsModDb.Models.Mods;

namespace VsModDb.Controllers;

[Authorize]
[ApiController]
[Route("api/v1/mods")]
public class ModsController(ModDbContext context) : ControllerBase
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
                Summary = f.Summary
            })
            .ToListAsync(cancellationToken: cancellationToken);

        return mods;
    }
}