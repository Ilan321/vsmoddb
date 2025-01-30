using System.Net.Mime;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VsModDb.Models.Assets;
using VsModDb.Models.Exceptions;
using VsModDb.Models.Mods;
using VsModDb.Models.Options;
using VsModDb.Models.Requests.Mods;
using VsModDb.Services.LegacyApi;
using VsModDb.Services.Mods;
using VsModDb.Services.Storage;

namespace VsModDb.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/mods/{alias}")]
public class ModController(
    ILogger<ModController> log,
    IOptions<LegacyClientOptions> legacyOptions,
    ILegacyApiClient apiClient,
    IModService modService
) : ModDbController
{
    [AllowAnonymous]
    [HttpGet("banner")]
    [ResponseCache(Duration = 60)]
    public async Task<Results<NotFound, FileStreamHttpResult>> GetPreviewBanner(
        [FromRoute] string alias,
        CancellationToken cancellationToken
    )
    {
        if (legacyOptions.Value.Enabled)
        {
            var logoStream = await apiClient.GetModLogoAsync(alias, cancellationToken);

            if (logoStream is null)
            {
                return TypedResults.NotFound();
            }

            return TypedResults.Stream(logoStream, MediaTypeNames.Image.Png);
        }

        var assetStream = await modService.GetBannerAsync(alias, cancellationToken);

        if (assetStream is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Stream(assetStream.Stream, assetStream.ContentType, fileDownloadName: assetStream.FileName);
    }

    [HttpPost("banner")]
    public async Task<Results<Ok, ForbidHttpResult, BadRequest>> SetPreviewBanner(
        [FromRoute] string alias,
        [FromForm] UpdateModBannerRequest request,
        CancellationToken cancellationToken
    )
    {
        if (legacyOptions.Value.Enabled)
        {
            throw new LegacyApiEnabledException();
        }

        log.LogInformation("Received request to update mod banner for mod {modId}", alias);

        if (!await modService.IsUserContributorAsync(alias, await CurrentUser, cancellationToken))
        {
            return TypedResults.Forbid();
        }

        await using var fileStream = request.File.OpenReadStream();

        await modService.SetBannerAsync(
            alias,
            new AssetStream(fileStream, request.File.FileName, request.File.ContentType),
            cancellationToken
        );

        return TypedResults.Ok();
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<Results<NotFound, Ok<ModDetailsDto>>> GetModDetails(
        [FromRoute] string alias,
        CancellationToken cancellationToken
    )
    {
        var mod = legacyOptions.Value.Enabled
        ? await apiClient.GetModAsync(alias, cancellationToken)
        : await modService.GetModDetailsAsync(alias, cancellationToken);

        if (mod is null)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.Ok(mod);
    }

    [AllowAnonymous]
    [HttpGet("comments")]
    public async Task<Results<NotFound, Ok<List<ModCommentDto>>>> GetModComments(
        [FromRoute] string alias,
        CancellationToken cancellationToken
    )
    {
        var comments = legacyOptions.Value.Enabled
            ? await apiClient.GetModCommentsAsync(alias, cancellationToken)
            : await modService.GetModCommentsAsync(alias, cancellationToken);

        return TypedResults.Ok(comments);
    }
}