namespace VsModDb.Models.Requests.Account;

public class SetAccountPasswordRequest
{
    public required string Password { get; init; }
    public required string LinkToken { get; init; }
}
