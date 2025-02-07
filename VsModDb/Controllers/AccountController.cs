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
    UserManager<User> userManager,
    SignInManager<User> signInManager
) : ModDbController
{
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

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task Login(LoginRequest request)
    {
        log.LogInformation("Received request to login for user {username}", request.Username);

        var user = await userManager.FindByNameAsync(request.Username);

        if (user is null)
        {
            log.LogWarning("Could not find user with username {username}", request.Username);

            throw new StatusCodeException(HttpStatusCode.Unauthorized);
        }

        var result = await signInManager.PasswordSignInAsync(
            user,
            request.Password,
            isPersistent: request.RememberMe,
            lockoutOnFailure: true
        );

        if (result.IsLockedOut)
        {
            log.LogWarning("User {username} is locked out", request.Username);

            throw new StatusCodeException(HttpStatusCode.Unauthorized);
        }

        if (result.IsNotAllowed)
        {
            log.LogWarning("User {username} is not allowed to login", request.Username);

            throw new StatusCodeException(HttpStatusCode.Unauthorized);
        }

        if (!result.Succeeded)
        {
            log.LogWarning("Failed to sign in user {username}", request.Username);

            throw new StatusCodeException(HttpStatusCode.Unauthorized);
        }
    }

    [HttpGet("logout")]
    public Task Logout() => signInManager.SignOutAsync();
}