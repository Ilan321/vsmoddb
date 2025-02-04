namespace VsModDb.Models.Responses.Account;

/// <summary>
/// Response for starting the account link process.
/// </summary>
public class StartAccountLinkResponse
{
    /// <summary>
    /// The token to use to link the account.
    /// The user is expected to put this token on a comment on the "linking" mod post (from configuration).
    /// </summary>
    public required string LinkToken { get; init; }

    /// <summary>
    /// The URL to the mod post where the verification token should be posted.
    /// </summary>
    public required string Url { get; init; }
}
