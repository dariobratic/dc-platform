using AdminApi.API.Models;
using AdminApi.API.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdminApi.API.Controllers;

[ApiController]
[Route("api/v1/admin")]
public class AdminController : ControllerBase
{
    private readonly IDirectoryServiceClient _directoryClient;
    private readonly IAuditServiceClient _auditClient;
    private readonly IServiceHealthChecker _healthChecker;
    private readonly ILogger<AdminController> _logger;

    public AdminController(
        IDirectoryServiceClient directoryClient,
        IAuditServiceClient auditClient,
        IServiceHealthChecker healthChecker,
        ILogger<AdminController> logger)
    {
        _directoryClient = directoryClient;
        _auditClient = auditClient;
        _healthChecker = healthChecker;
        _logger = logger;
    }

    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(DashboardResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<DashboardResponse>> GetDashboard(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching admin dashboard data");

        var orgCountTask = _directoryClient.GetOrganizationCountAsync(cancellationToken);
        var auditCountTask = _auditClient.GetTotalCountAsync(cancellationToken);

        await Task.WhenAll(orgCountTask, auditCountTask);

        var response = new DashboardResponse(
            OrganizationCount: await orgCountTask,
            AuditEntryCount: await auditCountTask,
            GeneratedAt: DateTime.UtcNow);

        return Ok(response);
    }

    [HttpGet("system/health")]
    [ProducesResponseType(typeof(SystemHealthResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<SystemHealthResponse>> GetSystemHealth(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Checking system health for all services");

        var health = await _healthChecker.CheckAllServicesAsync(cancellationToken);

        return Ok(health);
    }

    [HttpGet("organizations")]
    [ProducesResponseType(typeof(List<OrganizationSummary>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<OrganizationSummary>>> GetOrganizations(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching all organizations for admin view");

        var organizations = await _directoryClient.GetOrganizationsAsync(cancellationToken);

        return Ok(organizations);
    }

    [HttpGet("audit/recent")]
    [ProducesResponseType(typeof(List<AuditEntrySummary>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AuditEntrySummary>>> GetRecentAuditEntries(
        [FromQuery] int count = 20,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching {Count} recent audit entries for admin view", count);

        var entries = await _auditClient.GetRecentEntriesAsync(count, cancellationToken);

        return Ok(entries);
    }

    [HttpGet("health")]
    public ActionResult<HealthResponse> GetHealth()
    {
        return Ok(new HealthResponse(
            ServiceName: "AdminApi",
            Status: "Healthy",
            Timestamp: DateTime.UtcNow));
    }
}
