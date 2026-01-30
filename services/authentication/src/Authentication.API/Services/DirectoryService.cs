using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Authentication.API.Models;

namespace Authentication.API.Services;

public class DirectoryService : IDirectoryService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DirectoryService> _logger;

    public DirectoryService(HttpClient httpClient, ILogger<DirectoryService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<DirectoryOrganizationResponse> CreateOrganizationAsync(string name, string slug, string accessToken, CancellationToken cancellationToken = default)
    {
        var request = new { name, slug };
        using var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/organizations") { Content = content };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<DirectoryOrganizationResponse>(responseContent)
            ?? throw new InvalidOperationException("Failed to deserialize organization response");
    }

    public async Task<DirectoryWorkspaceResponse> CreateWorkspaceAsync(string organizationId, string name, string slug, string accessToken, CancellationToken cancellationToken = default)
    {
        var request = new { name, slug };
        using var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/organizations/{organizationId}/workspaces") { Content = content };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<DirectoryWorkspaceResponse>(responseContent)
            ?? throw new InvalidOperationException("Failed to deserialize workspace response");
    }

    public async Task<DirectoryMembershipResponse> AddMemberAsync(string workspaceId, string userId, string role, string accessToken, CancellationToken cancellationToken = default)
    {
        var request = new { userId, role };
        using var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");
        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, $"/api/v1/workspaces/{workspaceId}/members") { Content = content };
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(httpRequest, cancellationToken);
        response.EnsureSuccessStatusCode();

        var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
        return JsonSerializer.Deserialize<DirectoryMembershipResponse>(responseContent)
            ?? throw new InvalidOperationException("Failed to deserialize membership response");
    }
}
