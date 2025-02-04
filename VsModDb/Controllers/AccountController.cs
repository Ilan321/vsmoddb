using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VsModDb.Constants;
using VsModDb.Data.Entities;
using VsModDb.Models.Exceptions;
using VsModDb.Models.Requests.Account;
using VsModDb.Models.Responses.Account;
using VsModDb.Services.Account;

namespace VsModDb.Controllers;

[Authorize]
[Route("api/v1/account")]
public class AccountController(
    ILogger<AccountController> log,
    IAccountService accountService,
    UserManager<User> userManager
) : ModDbController
{
    [AllowAnonymous]
    [HttpPost("link/start")]
    public async Task<StartAccountLinkResponse> StartAccountLink(
        [FromBody] StartAccountLinkRequest request,
        CancellationToken cancellationToken
    )
    {
        log.LogInformation("Received request to start account link for user {username}", request.Username);

        var existingUser = await userManager.FindByNameAsync(request.Username);

        if (existingUser is not null)
        {
            log.LogWarning("User already exists with name {username}", request.Username);

            throw new StatusCodeException(HttpStatusCode.BadRequest, ErrorCodes.Account.ACCOUNT_ALREADY_EXISTS);
        }

        var token = await accountService.StartAccountLinkAsync(
            request.Username,
            request.Email,
            cancellationToken
        );

        log.LogDebug("Successfully stored account link request for user {username}", request.Username);

        return new()
        {
            LinkToken = token
        };
    }

    [AllowAnonymous]
    [HttpPost("link/verify")]
    public async Task<VerifyAccountLinkResponse> VerifyAccountLink(
        [FromBody] VerifyAccountLinkRequest request,
        CancellationToken cancellationToken
    )
    {
        log.LogInformation("Received request to verify account linkn for token {token}", request.LinkToken);

        var success = await accountService.VerifyAccountLinkAsync(
            request.LinkToken,
            cancellationToken
        );

        return new()
        {
            Success = success
        };
    }
}
