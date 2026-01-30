using AdminApi.API.Models;

namespace AdminApi.API.Services;

public class DirectoryServiceClient : IDirectoryServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DirectoryServiceClient> _logger;

    public DirectoryServiceClient(HttpClient httpClient, ILogger<DirectoryServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<OrganizationSummary>> GetOrganizationsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching organizations from Directory service");

        try
        {
            var response = await _httpClient.GetAsync("/api/v1/organizations", cancellationToken);
            response.EnsureSuccessStatusCode();

            var organizations = await response.Content.ReadFromJsonAsync<List<OrganizationSummary>>(cancellationToken);
            return organizations ?? [];
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to fetch organizations from Directory service");
            return [];
        }
    }

    public async Task<int> GetOrganizationCountAsync(CancellationToken cancellationToken = default)
    {
        var organizations = await GetOrganizationsAsync(cancellationToken);
        return organizations.Count;
    }
}
