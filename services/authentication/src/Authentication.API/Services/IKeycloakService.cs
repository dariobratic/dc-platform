using Authentication.API.Models;

namespace Authentication.API.Services;

public interface IKeycloakService
{
    Task<TokenResponse> ExchangeCodeForTokenAsync(string code, string redirectUri, CancellationToken cancellationToken = default);
    Task<TokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<string> CreateUserAsync(string email, string password, string firstName, string lastName, CancellationToken cancellationToken = default);
    Task<TokenResponse> AuthenticateWithPasswordAsync(string username, string password, CancellationToken cancellationToken = default);
    Task UpdateUserAttributesAsync(string userId, Dictionary<string, List<string>> attributes, CancellationToken cancellationToken = default);
}
