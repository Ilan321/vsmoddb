using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using VsModDb.Data.Entities;
using VsModDb.Models.Tasks;

namespace VsModDb.Controllers;

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
            throw new InvalidOperationException($"User {User.Identity?.Name} not found");
        }

        return user;
    }
}
