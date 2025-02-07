namespace VsModDb.Models.Requests.Account;

public class LoginRequest
{
    public required string Username { get; init; }
    public required string Password { get; init; }
    public bool RememberMe { get; init; }
}
