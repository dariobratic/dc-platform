using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using AdminApi.API.Models;
using AdminApi.API.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace AdminApi.Tests.Services;

public class DirectoryServiceClientTests
{
    private readonly ILogger<DirectoryServiceClient> _logger = Substitute.For<ILogger<DirectoryServiceClient>>();

    private DirectoryServiceClient CreateClient(HttpMessageHandler handler)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://localhost:5001")
        };
        return new DirectoryServiceClient(httpClient, _logger);
    }

    [Fact]
    public async Task GetOrganizationsAsync_ReturnsOrganizations()
    {
        // Arrange
        var orgs = new List<OrganizationSummary>
        {
            new(Guid.NewGuid(), "Org 1", "org-1", "Active", DateTime.UtcNow),
            new(Guid.NewGuid(), "Org 2", "org-2", "Active", DateTime.UtcNow)
        };

        var handler = new MockHttpMessageHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(orgs)
            }));

        var client = CreateClient(handler);

        // Act
        var result = await client.GetOrganizationsAsync();

        // Assert
        result.Should().HaveCount(2);
        result[0].Name.Should().Be("Org 1");
    }

    [Fact]
    public async Task GetOrganizationsAsync_WhenServiceUnavailable_ReturnsEmptyList()
    {
        // Arrange
        var handler = new MockHttpMessageHandler(_ =>
            throw new HttpRequestException("Service unavailable"));

        var client = CreateClient(handler);

        // Act
        var result = await client.GetOrganizationsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetOrganizationCountAsync_ReturnsCount()
    {
        // Arrange
        var orgs = new List<OrganizationSummary>
        {
            new(Guid.NewGuid(), "Org 1", "org-1", "Active", DateTime.UtcNow),
            new(Guid.NewGuid(), "Org 2", "org-2", "Active", DateTime.UtcNow),
            new(Guid.NewGuid(), "Org 3", "org-3", "Active", DateTime.UtcNow)
        };

        var handler = new MockHttpMessageHandler(_ =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(orgs)
            }));

        var client = CreateClient(handler);

        // Act
        var count = await client.GetOrganizationCountAsync();

        // Assert
        count.Should().Be(3);
    }
}

public class MockHttpMessageHandler : HttpMessageHandler
{
    private readonly Func<HttpRequestMessage, Task<HttpResponseMessage>> _sendAsync;

    public MockHttpMessageHandler(Func<HttpRequestMessage, Task<HttpResponseMessage>> sendAsync)
    {
        _sendAsync = sendAsync;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return _sendAsync(request);
    }
}
