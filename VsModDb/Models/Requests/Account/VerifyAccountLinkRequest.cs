namespace VsModDb.Models.Requests.Account;

public class VerifyAccountLinkRequest
{
    public required string LinkToken { get; init; }
}