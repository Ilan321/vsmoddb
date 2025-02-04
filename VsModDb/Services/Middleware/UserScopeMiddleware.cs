using Microsoft.AspNetCore.Identity;
using VsModDb.Data.Entities;

namespace VsModDb.Services.Middleware;

public class UserScopeMiddleware(ILogger<UserScopeMiddleware> log, UserManager<User> userManager) : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var user = await userManager.GetUserAsync(context.User);

        using var scope = CreateUserScope(user);

        await next(context);
    }

    private IDisposable? CreateUserScope(User? user)
    {
        var dict = new Dictionary<string, object>();

        if (user is not null)
        {
            dict.Add("User", new
            {
                UserId = user.Id,
                Username = user.UserName,
                ModDbId = user.ModDbUserId
            });
        }

        return log.BeginScope(dict);
    }
}
