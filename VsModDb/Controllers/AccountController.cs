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

    [HttpGet("logout")]
    public Task Logout() => signInManager.SignOutAsync();
}