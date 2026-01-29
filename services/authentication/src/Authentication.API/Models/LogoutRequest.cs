namespace Authentication.API.Models;

public record LogoutRequest
{
    public required string RefreshToken { get; init; }
}
