using System.Text.Json.Serialization;

namespace Authentication.API.Models;

internal record KeycloakTokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; init; } = string.Empty;

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; init; } = string.Empty;

    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }

    [JsonPropertyName("token_type")]
    public string TokenType { get; init; } = string.Empty;

    public TokenResponse ToApiResponse() =>
        new(AccessToken, RefreshToken, ExpiresIn, TokenType);
}
