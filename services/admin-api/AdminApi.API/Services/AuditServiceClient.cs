using AdminApi.API.Models;

namespace AdminApi.API.Services;

public class AuditServiceClient : IAuditServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuditServiceClient> _logger;

    public AuditServiceClient(HttpClient httpClient, ILogger<AuditServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<List<AuditEntrySummary>> GetRecentEntriesAsync(int count = 20, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching {Count} recent audit entries from Audit service", count);

        try
        {
            var response = await _httpClient.GetAsync($"/api/v1/audit?pageSize={count}&page=1", cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<AuditPagedResponse>(cancellationToken);
            return result?.Items ?? [];
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to fetch audit entries from Audit service");
            return [];
        }
    }

    public async Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync("/api/v1/audit?pageSize=1&page=1", cancellationToken);
            response.EnsureSuccessStatusCode();

            var result = await response.Content.ReadFromJsonAsync<AuditPagedResponse>(cancellationToken);
            return result?.TotalCount ?? 0;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Failed to fetch audit count from Audit service");
            return 0;
        }
    }
}

// Internal DTO for deserializing the paged response from Audit service
internal record AuditPagedResponse(
    List<AuditEntrySummary> Items,
    int TotalCount,
    int Page,
    int PageSize);
