namespace VsModDb.Models.Responses.Account;

public class GetUserProfileResponse
{
    public required string Username { get; init; }
    public required string Email { get; init; }
}
