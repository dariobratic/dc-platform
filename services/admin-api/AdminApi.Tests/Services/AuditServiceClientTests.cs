using System.Net;
using System.Net.Http.Json;
using AdminApi.API.Models;
using AdminApi.API.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace AdminApi.Tests.Services;

public class AuditServiceClientTests
{
    private readonly ILogger<AuditServiceClient> _logger = Substitute.For<ILogger<AuditServiceClient>>();

    private AuditServiceClient CreateClient(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:5004")
        };
        return new AuditServiceClient(httpClient, _logger);
    }

    [Fact]
    public async Task GetRecentEntriesAsync_ReturnsEntries()
    {
        // Arrange
        var pagedResponse = new
        {
            Items = new List<AuditEntrySummary>
            {
                new(Guid.NewGuid(), "org.created", "Organization", Guid.NewGuid(), Guid.NewGuid(), "user@test.com", "Directory", DateTime.UtcNow)
            },
            TotalCount = 1,
            Page = 1,
            PageSize = 20
        };

        var handler = new MockHttpMessageHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(pagedResponse)
            }));

        var client = CreateClient(handler);

        // Act
        var result = await client.GetRecentEntriesAsync(20);

        // Assert
        result.Should().HaveCount(1);
        result[0].Action.Should().Be("org.created");
    }

    [Fact]
    public async Task GetRecentEntriesAsync_WhenServiceUnavailable_ReturnsEmptyList()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(_ =>
            throw new HttpRequestException("Service unavailable"));

        var client = CreateClient(handler);

        // Act
        var result = await client.GetRecentEntriesAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTotalCountAsync_ReturnsCount()
    {
        // Arrange
        var pagedResponse = new
        {
            Items = new List<AuditEntrySummary>(),
            TotalCount = 42,
            Page = 1,
            PageSize = 1
        };

        var handler = new MockHttpMessageHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(pagedResponse)
            }));

        var client = CreateClient(handler);

        // Act
        var count = await client.GetTotalCountAsync();

        // Assert
        count.Should().Be(42);
    }

    [Fact]
    public async Task GetTotalCountAsync_WhenServiceUnavailable_ReturnsZero()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(_ =>
            throw new HttpRequestException("Service unavailable"));

        var client = CreateClient(handler);

        // Act
        var count = await client.GetTotalCountAsync();

        // Assert
        count.Should().Be(0);
    }
}
