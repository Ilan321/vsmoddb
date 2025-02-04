using ilandev.Extensions.Configuration;

namespace VsModDb.Models.Options;

public class AccountOptions : IAppOptions
{
    public static string Section => "Account";

    /// <summary>
    /// Gets how long an account link token is valid for, in minutes.
    /// </summary>
    public required int LinkTokenExpirationMinutes { get; init; } = 10;

    /// <summary>
    /// Gets the URL of the mod post to check the comments for, when linking accounts.
    /// </summary>
    public required string LinkTokenModPostUrl { get; init; }
}
