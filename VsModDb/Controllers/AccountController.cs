using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using VsModDb.Constants;
using VsModDb.Data.Entities;
using VsModDb.Models.Exceptions;
using VsModDb.Models.Options;
using VsModDb.Models.Requests.Account;
using VsModDb.Models.Responses.Account;
using VsModDb.Services.Account;

namespace VsModDb.Controllers;

[Authorize]
[Route("api/v1/account")]
public class AccountController(
    ILogger<AccountController> log,
    IAccountService accountService,
    UserManager<User> userManager,
    IOptions<AccountOptions> accountOptions
) : ModDbController
{
    private const string SecretLinkTokenCookieName = ".LinkToken";

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

        var details = await accountService.StartAccountLinkAsync(
            request.Username,
            request.Email,
            cancellationToken
        );

        log.LogDebug("Successfully stored account link request for user {username}", request.Username);

        HttpContext.Response.Cookies.Append(
            SecretLinkTokenCookieName,
            details.Secret,
            new CookieOptions
            {
                MaxAge = TimeSpan.FromMinutes(accountOptions.Value.LinkTokenExpirationMinutes),
                HttpOnly = true
            }
        );

        return new()
        {
            LinkToken = details.Token,
            Url = accountOptions.Value.LinkTokenModPostUrl
        };
    }

    [AllowAnonymous]
    [HttpPost("link/verify")]
    public async Task VerifyAccountLink(
        [FromBody] VerifyAccountLinkRequest request,
        CancellationToken cancellationToken
    )
    {
        log.LogInformation("Received request to verify account link for token {token}", request.LinkToken);

        var secret = GetSecretLinkToken();

        await accountService.VerifyAccountLinkAsync(
            request.LinkToken,
            secret,
            cancellationToken
        );
    }

    [HttpPost("link/set-password")]
    public async Task SetAccountPassword(
        [FromBody] SetAccountPasswordRequest request,
        CancellationToken cancellationToken
    )
    {
        var user = await CurrentUser;

        log.LogInformation("Received request to set account password for user {username}", user.UserName);

        await accountService.SetAccountPasswordAsync(
            user,
            request.Password,
            request.LinkToken,
            GetSecretLinkToken(),
            cancellationToken
        );

        HttpContext.Response.Cookies.Delete(SecretLinkTokenCookieName);
    }

    [HttpGet("profile")]
    public async Task<GetUserProfileResponse> GetUserProfile()
    {
        var user = await CurrentUser;

        return new()
        {
            Username = user.UserName!,
            Email = user.Email!
        };
    }

    private string? GetSecretLinkToken() => HttpContext
        .Request
        .Cookies
        .TryGetValue(SecretLinkTokenCookieName, out var secretToken)
        ? secretToken
        : null;
}