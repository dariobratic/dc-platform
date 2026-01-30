using System.Text.Json;
using Authentication.API.Models;

namespace Authentication.API.Services;

public class KeycloakService : IKeycloakService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KeycloakService> _logger;

    private string BaseUrl => _configuration["Keycloak:BaseUrl"] ?? throw new InvalidOperationException("Keycloak:BaseUrl not configured");
    private string Realm => _configuration["Keycloak:Realm"] ?? throw new InvalidOperationException("Keycloak:Realm not configured");
    private string ClientId => _configuration["Keycloak:ClientId"] ?? throw new InvalidOperationException("Keycloak:ClientId not configured");
    private string ClientSecret => _configuration["Keycloak:ClientSecret"] ?? throw new InvalidOperationException("Keycloak:ClientSecret not configured");
    private string FrontendClientId => _configuration["Keycloak:FrontendClientId"] ?? throw new InvalidOperationException("Keycloak:FrontendClientId not configured");

    private string TokenEndpoint => $"{BaseUrl}/realms/{Realm}/protocol/openid-connect/token";
    private string RevokeEndpoint => $"{BaseUrl}/realms/{Realm}/protocol/openid-connect/revoke";
    private string AdminUsersEndpoint => $"{BaseUrl}/admin/realms/{Realm}/users";

    public KeycloakService(HttpClient httpClient, IConfiguration configuration, ILogger<KeycloakService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<TokenResponse> ExchangeCodeForTokenAsync(string code, string redirectUri, CancellationToken cancellationToken = default)
    {
        var requestData = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["client_id"] = ClientId,
            ["client_secret"] = ClientSecret,
            ["code"] = code,
            ["redirect_uri"] = redirectUri
        };

        return await SendTokenRequestAsync(requestData, cancellationToken);
    }

    public async Task<TokenResponse> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var requestData = new Dictionary<string, string>
        {
            ["grant_type"] = "refresh_token",
            ["client_id"] = ClientId,
            ["client_secret"] = ClientSecret,
            ["refresh_token"] = refreshToken
        };

        return await SendTokenRequestAsync(requestData, cancellationToken);
    }

    public async Task RevokeTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        var requestData = new Dictionary<string, string>
        {
            ["client_id"] = ClientId,
            ["client_secret"] = ClientSecret,
            ["token"] = refreshToken,
            ["token_type_hint"] = "refresh_token"
        };

        try
        {
            using var content = new FormUrlEncodedContent(requestData);
            var response = await _httpClient.PostAsync(RevokeEndpoint, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("Failed to revoke token. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, errorContent);

                // Revoking a token is best-effort, so we don't throw on failure
                // The token will expire naturally anyway
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error occurred while revoking token");
            // Don't throw - revoke is best-effort
        }
    }

    public async Task<string> CreateUserAsync(string email, string password, string firstName, string lastName, CancellationToken cancellationToken = default)
    {
        var adminToken = await GetServiceAccountTokenAsync(cancellationToken);

        var userRepresentation = new
        {
            username = email,
            email,
            firstName,
            lastName,
            enabled = true,
            emailVerified = true,
            credentials = new[]
            {
                new
                {
                    type = "password",
                    value = password,
                    temporary = false
                }
            }
        };

        var json = JsonSerializer.Serialize(userRepresentation);
        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        using var request = new HttpRequestMessage(HttpMethod.Post, AdminUsersEndpoint) { Content = content };
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);

        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            throw new InvalidOperationException("A user with this email already exists");
        }

        response.EnsureSuccessStatusCode();

        // Extract user ID from Location header: .../users/{id}
        var locationHeader = response.Headers.Location?.ToString();
        if (locationHeader == null)
        {
            throw new InvalidOperationException("Keycloak did not return a Location header for created user");
        }

        return locationHeader.Split('/').Last();
    }

    public async Task<TokenResponse> AuthenticateWithPasswordAsync(string username, string password, CancellationToken cancellationToken = default)
    {
        var requestData = new Dictionary<string, string>
        {
            ["grant_type"] = "password",
            ["client_id"] = FrontendClientId,
            ["username"] = username,
            ["password"] = password,
            ["scope"] = "openid profile email"
        };

        return await SendTokenRequestAsync(requestData, cancellationToken);
    }

    public async Task UpdateUserAttributesAsync(string userId, Dictionary<string, List<string>> attributes, CancellationToken cancellationToken = default)
    {
        var adminToken = await GetServiceAccountTokenAsync(cancellationToken);

        var userUrl = $"{AdminUsersEndpoint}/{userId}";

        // First, get existing user to merge attributes
        using var getRequest = new HttpRequestMessage(HttpMethod.Get, userUrl);
        getRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);
        var getResponse = await _httpClient.SendAsync(getRequest, cancellationToken);
        getResponse.EnsureSuccessStatusCode();

        var userJson = await getResponse.Content.ReadAsStringAsync(cancellationToken);
        using var userDoc = JsonDocument.Parse(userJson);
        var existingAttributes = new Dictionary<string, List<string>>();

        if (userDoc.RootElement.TryGetProperty("attributes", out var attrElement))
        {
            foreach (var prop in attrElement.EnumerateObject())
            {
                var values = new List<string>();
                foreach (var val in prop.Value.EnumerateArray())
                {
                    values.Add(val.GetString() ?? string.Empty);
                }
                existingAttributes[prop.Name] = values;
            }
        }

        // Merge new attributes
        foreach (var kvp in attributes)
        {
            existingAttributes[kvp.Key] = kvp.Value;
        }

        var updatePayload = new { attributes = existingAttributes };
        var json = JsonSerializer.Serialize(updatePayload);
        using var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        using var putRequest = new HttpRequestMessage(HttpMethod.Put, userUrl) { Content = content };
        putRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", adminToken);

        var putResponse = await _httpClient.SendAsync(putRequest, cancellationToken);
        putResponse.EnsureSuccessStatusCode();
    }

    private async Task<string> GetServiceAccountTokenAsync(CancellationToken cancellationToken)
    {
        var requestData = new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = ClientId,
            ["client_secret"] = ClientSecret
        };

        var tokenResponse = await SendTokenRequestAsync(requestData, cancellationToken);
        return tokenResponse.AccessToken;
    }

    private async Task<TokenResponse> SendTokenRequestAsync(Dictionary<string, string> requestData, CancellationToken cancellationToken)
    {
        try
        {
            using var content = new FormUrlEncodedContent(requestData);
            var response = await _httpClient.PostAsync(TokenEndpoint, content, cancellationToken);

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Token request failed. Status: {StatusCode}, Response: {Response}",
                    response.StatusCode, responseContent);

                throw response.StatusCode switch
                {
                    System.Net.HttpStatusCode.BadRequest => new HttpRequestException("Invalid grant or bad request", null, response.StatusCode),
                    System.Net.HttpStatusCode.Unauthorized => new HttpRequestException("Invalid client credentials", null, response.StatusCode),
                    _ => new HttpRequestException($"Token request failed with status {response.StatusCode}", null, response.StatusCode)
                };
            }

            var keycloakResponse = JsonSerializer.Deserialize<KeycloakTokenResponse>(responseContent);

            if (keycloakResponse == null)
            {
                throw new InvalidOperationException("Failed to deserialize token response");
            }

            return keycloakResponse.ToApiResponse();
        }
        catch (HttpRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred during token request");
            throw new InvalidOperationException("An error occurred while communicating with the authentication provider", ex);
        }
    }
}
