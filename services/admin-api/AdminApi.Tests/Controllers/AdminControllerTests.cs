using AdminApi.API.Controllers;
using AdminApi.API.Models;
using AdminApi.API.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace AdminApi.Tests.Controllers;

public class AdminControllerTests
{
    private readonly IDirectoryServiceClient _directoryClient = Substitute.For<IDirectoryServiceClient>();
    private readonly IAuditServiceClient _auditClient = Substitute.For<IAuditServiceClient>();
    private readonly IServiceHealthChecker _healthChecker = Substitute.For<IServiceHealthChecker>();
    private readonly ILogger<AdminController> _logger = Substitute.For<ILogger<AdminController>>();
    private readonly AdminController _controller;

    public AdminControllerTests()
    {
        _controller = new AdminController(_directoryClient, _auditClient, _healthChecker, _logger);
    }

    [Fact]
    public async Task GetDashboard_ReturnsAggregatedData()
    {
        // Arrange
        _directoryClient.GetOrganizationCountAsync(Arg.Any<CancellationToken>()).Returns(5);
        _auditClient.GetTotalCountAsync(Arg.Any<CancellationToken>()).Returns(100);

        // Act
        var result = await _controller.GetDashboard(CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var dashboard = okResult.Value.Should().BeOfType<DashboardResponse>().Subject;
        dashboard.OrganizationCount.Should().Be(5);
        dashboard.AuditEntryCount.Should().Be(100);
        dashboard.GeneratedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public async Task GetSystemHealth_ReturnsServiceStatuses()
    {
        // Arrange
        var healthResponse = new SystemHealthResponse(
            OverallStatus: "Healthy",
            Services:
            [
                new ServiceHealthStatus("Directory", "Healthy", 200, null),
                new ServiceHealthStatus("Audit", "Healthy", 200, null)
            ],
            CheckedAt: DateTime.UtcNow);

        _healthChecker.CheckAllServicesAsync(Arg.Any<CancellationToken>()).Returns(healthResponse);

        // Act
        var result = await _controller.GetSystemHealth(CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var health = okResult.Value.Should().BeOfType<SystemHealthResponse>().Subject;
        health.OverallStatus.Should().Be("Healthy");
        health.Services.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetOrganizations_ReturnsList()
    {
        // Arrange
        var orgs = new List<OrganizationSummary>
        {
            new(Guid.NewGuid(), "Org 1", "org-1", "Active", DateTime.UtcNow),
            new(Guid.NewGuid(), "Org 2", "org-2", "Active", DateTime.UtcNow)
        };
        _directoryClient.GetOrganizationsAsync(Arg.Any<CancellationToken>()).Returns(orgs);

        // Act
        var result = await _controller.GetOrganizations(CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var organizations = okResult.Value.Should().BeAssignableTo<List<OrganizationSummary>>().Subject;
        organizations.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetOrganizations_WhenServiceDown_ReturnsEmptyList()
    {
        // Arrange
        _directoryClient.GetOrganizationsAsync(Arg.Any<CancellationToken>())
            .Returns(new List<OrganizationSummary>());

        // Act
        var result = await _controller.GetOrganizations(CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var organizations = okResult.Value.Should().BeAssignableTo<List<OrganizationSummary>>().Subject;
        organizations.Should().BeEmpty();
    }

    [Fact]
    public async Task GetRecentAuditEntries_ReturnsList()
    {
        // Arrange
        var entries = new List<AuditEntrySummary>
        {
            new(Guid.NewGuid(), "org.created", "Organization", Guid.NewGuid(), Guid.NewGuid(), "user@test.com", "Directory", DateTime.UtcNow)
        };
        _auditClient.GetRecentEntriesAsync(20, Arg.Any<CancellationToken>()).Returns(entries);

        // Act
        var result = await _controller.GetRecentAuditEntries(20, CancellationToken.None);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var auditEntries = okResult.Value.Should().BeAssignableTo<List<AuditEntrySummary>>().Subject;
        auditEntries.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetRecentAuditEntries_WithCustomCount_PassesCount()
    {
        // Arrange
        _auditClient.GetRecentEntriesAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new List<AuditEntrySummary>());

        // Act
        await _controller.GetRecentAuditEntries(50, CancellationToken.None);

        // Assert
        await _auditClient.Received(1).GetRecentEntriesAsync(50, Arg.Any<CancellationToken>());
    }

    [Fact]
    public void GetHealth_ReturnsHealthyStatus()
    {
        // Act
        var result = _controller.GetHealth();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Subject;
        var health = okResult.Value.Should().BeOfType<HealthResponse>().Subject;
        health.ServiceName.Should().Be("AdminApi");
        health.Status.Should().Be("Healthy");
    }
}
