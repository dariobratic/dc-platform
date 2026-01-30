using System.Net;
using AdminApi.API.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace AdminApi.Tests.Services;

public class ServiceHealthCheckerTests
{
    private readonly ILogger<ServiceHealthChecker> _logger = Substitute.For<ILogger<ServiceHealthChecker>>();

    private readonly IConfiguration _configuration = new ConfigurationBuilder()
        .AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ServiceUrls:Directory"] = "http://localhost:5001",
            ["ServiceUrls:Authentication"] = "http://localhost:5002",
            ["ServiceUrls:AccessControl"] = "http://localhost:5003",
            ["ServiceUrls:Audit"] = "http://localhost:5004",
            ["ServiceUrls:Notification"] = "http://localhost:5005",
            ["ServiceUrls:Configuration"] = "http://localhost:5006"
        })
        .Build();

    [Fact]
    public async Task CheckAllServicesAsync_WhenAllHealthy_ReturnsHealthyOverallStatus()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)));

        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient(Arg.Any<string>())
            .Returns(_ => new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(5) });

        var checker = new ServiceHealthChecker(httpClientFactory, _configuration, _logger);

        // Act
        var result = await checker.CheckAllServicesAsync();

        // Assert
        result.OverallStatus.Should().Be("Healthy");
        result.Services.Should().HaveCount(7); // Gateway + 6 services
        result.Services.Should().AllSatisfy(s => s.Status.Should().Be("Healthy"));
    }

    [Fact]
    public async Task CheckAllServicesAsync_WhenOneServiceDown_ReturnsDegradedStatus()
    {
        // Arrange
        var callCount = 0;
        var handler = new MockHttpMessageHandler(_ =>
        {
            callCount++;
            if (callCount == 1)
                throw new HttpRequestException("Connection refused");
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        });

        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient(Arg.Any<string>())
            .Returns(_ => new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(5) });

        var checker = new ServiceHealthChecker(httpClientFactory, _configuration, _logger);

        // Act
        var result = await checker.CheckAllServicesAsync();

        // Assert
        result.OverallStatus.Should().Be("Degraded");
        result.Services.Should().Contain(s => s.Status == "Unreachable");
    }

    [Fact]
    public async Task CheckAllServicesAsync_WhenServiceUnreachable_MarksAsUnreachable()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(_ =>
            throw new HttpRequestException("Connection refused"));

        var httpClientFactory = Substitute.For<IHttpClientFactory>();
        httpClientFactory.CreateClient(Arg.Any<string>())
            .Returns(_ => new HttpClient(handler) { Timeout = TimeSpan.FromSeconds(5) });

        var checker = new ServiceHealthChecker(httpClientFactory, _configuration, _logger);

        // Act
        var result = await checker.CheckAllServicesAsync();

        // Assert
        result.OverallStatus.Should().Be("Degraded");
        result.Services.Should().AllSatisfy(s =>
        {
            s.Status.Should().Be("Unreachable");
            s.StatusCode.Should().BeNull();
        });
    }
}
