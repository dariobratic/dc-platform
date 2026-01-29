namespace Authentication.API.Models;

public record UserInfoResponse
{
    public required string Sub { get; init; }
    public required string Email { get; init; }
    public bool EmailVerified { get; init; }
    public required string PreferredUsername { get; init; }
}
