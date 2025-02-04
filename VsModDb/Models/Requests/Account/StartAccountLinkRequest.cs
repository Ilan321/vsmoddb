namespace VsModDb.Models.Requests.Account;

/// <summary>
/// Used for starting the linking process between the user's original ModDB account and their account here.
/// </summary>
public class StartAccountLinkRequest
{
    public required string Username { get; init; }
    public required string Email { get; init; }
}
