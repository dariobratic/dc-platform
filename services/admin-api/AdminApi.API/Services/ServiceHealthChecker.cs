using AdminApi.API.Models;

namespace AdminApi.API.Services;

public class ServiceHealthChecker : IServiceHealthChecker
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<ServiceHealthChecker> _logger;

    public ServiceHealthChecker(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<ServiceHealthChecker> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<SystemHealthResponse> CheckAllServicesAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Starting system health check for all services");

        var serviceUrls = new Dictionary<string, string>
        {
            ["Gateway"] = "http://localhost:5000",
            ["Directory"] = _configuration["ServiceUrls:Directory"]!,
            ["Authentication"] = _configuration["ServiceUrls:Authentication"]!,
            ["AccessControl"] = _configuration["ServiceUrls:AccessControl"]!,
            ["Audit"] = _configuration["ServiceUrls:Audit"]!,
            ["Notification"] = _configuration["ServiceUrls:Notification"]!,
            ["Configuration"] = _configuration["ServiceUrls:Configuration"]!
        };

        var checks = serviceUrls.Select(async kvp =>
        {
            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(5);

            try
            {
                var response = await client.GetAsync($"{kvp.Value}/health", cancellationToken);
                return new ServiceHealthStatus(
                    ServiceName: kvp.Key,
                    Status: response.IsSuccessStatusCode ? "Healthy" : "Unhealthy",
                    StatusCode: (int)response.StatusCode,
                    ResponseTimeMs: null);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Health check failed for {Service}", kvp.Key);
                return new ServiceHealthStatus(
                    ServiceName: kvp.Key,
                    Status: "Unreachable",
                    StatusCode: null,
                    ResponseTimeMs: null);
            }
        });

        var results = await Task.WhenAll(checks);

        var allHealthy = results.All(r => r.Status == "Healthy");

        return new SystemHealthResponse(
            OverallStatus: allHealthy ? "Healthy" : "Degraded",
            Services: results.ToList(),
            CheckedAt: DateTime.UtcNow);
    }
}
