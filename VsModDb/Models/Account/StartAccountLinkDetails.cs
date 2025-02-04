namespace VsModDb.Models.Account;

public class StartAccountLinkDetails
{
    public required string Token { get; init; }
    public required string Secret { get; init; }
}
