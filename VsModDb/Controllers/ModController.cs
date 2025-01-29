using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using VsModDb.Models.Assets;
using VsModDb.Models.Requests.Mods;
using VsModDb.Services.Mods;
using VsModDb.Services.Storage;

namespace VsModDb.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/mods/{mod-id:int}")]
public class ModController(
    ILogger<ModController> log,
    IModService modService
) : ModDbController
{
    [AllowAnonymous]
    [HttpGet("banner")]
    [ResponseCache(Duration = 60)]
    public async Task<Results<NotFound, FileStreamHttpResult>> GetPreviewBanner([FromRoute(Name = "mod-id")] int modId,
        CancellationToken cancellationToken)
    {
        var assetStream = await modService.GetBannerAsync(modId, cancellationToken);

        if (assetStream is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Stream(assetStream.Stream, assetStream.ContentType, fileDownloadName: assetStream.FileName);
    }

    [HttpPost("banner")]
    public async Task<Results<Ok, ForbidHttpResult, BadRequest>> SetPreviewBanner(
        [FromRoute(Name = "mod-id")] int modId, [FromForm] UpdateModBannerRequest request, CancellationToken cancellationToken)
    {
        log.LogInformation("Received request to update mod banner for mod {modId}", modId);

        if (!await modService.IsUserContributorAsync(modId, await CurrentUser, cancellationToken))
        {
            return TypedResults.Forbid();
        }

        await using var fileStream = request.File.OpenReadStream();

        await modService.SetBannerAsync(
            modId,
            new AssetStream(fileStream, request.File.FileName, request.File.ContentType),
            cancellationToken
        );

        return TypedResults.Ok();
    }
}