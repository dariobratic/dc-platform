namespace Authentication.API.Models;

public record RefreshTokenRequest
{
    public required string RefreshToken { get; init; }
}
