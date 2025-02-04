using System.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VsModDb.Data.Entities;
using VsModDb.Models.Exceptions;
using VsModDb.Models.Tasks;

namespace VsModDb.Controllers;

[ApiController]
public abstract class ModDbController : ControllerBase
{
    public AsyncLazy<User> CurrentUser { get; }

    public ModDbController()
    {
        CurrentUser = new(GetCurrentUser);
    }

    private async Task<User> GetCurrentUser()
    {
        var userManager = HttpContext.RequestServices.GetRequiredService<UserManager<User>>();

        var user = await userManager.GetUserAsync(User);

        if (user is null)
        {
            // Could not get user, remove cookie and return 401

            var signInManager = HttpContext.RequestServices.GetRequiredService<SignInManager<User>>();

            await signInManager.SignOutAsync();

            throw new StatusCodeException(HttpStatusCode.Unauthorized);
        }

        return user;
    }
}
