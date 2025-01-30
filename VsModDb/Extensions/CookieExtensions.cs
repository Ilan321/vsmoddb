using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using System.Net;

namespace VsModDb.Extensions;

public static class CookieExtensions
{
    public static void ConfigureApiDefaults(this CookieAuthenticationOptions opts)
    {
        opts.Events = new()
        {
            OnRedirectToAccessDenied = c => OnRedirect(c, HttpStatusCode.Forbidden),
            OnRedirectToLogin = c => OnRedirect(c, HttpStatusCode.Unauthorized)
        };
    }

    private static Task OnRedirect(RedirectContext<CookieAuthenticationOptions> context, HttpStatusCode statusCode)
    {
        context.Response.StatusCode = (int)statusCode;

        return Task.CompletedTask;
    }
}
