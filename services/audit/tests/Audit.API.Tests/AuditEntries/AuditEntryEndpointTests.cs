using System.Net;
using System.Net.Http.Json;
using Audit.API.Tests.Extensions;
using Audit.API.Tests.Fixtures;
using FluentAssertions;

namespace Audit.API.Tests.AuditEntries;

[Collection("Integration")]
[Trait("Category", "Integration")]
public class AuditEntryEndpointTests
{
    private readonly HttpClient _client;

    public AuditEntryEndpointTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Client;
    }

    private static object CreateAuditEntryRequest(
        Guid? userId = null,
        string action = "organization.created",
        string entityType = "Organization",
        Guid? entityId = null,
        string serviceName = "Directory",
        Guid? organizationId = null)
    {
        return new
        {
            UserId = userId ?? Guid.NewGuid(),
            Action = action,
            EntityType = entityType,
            EntityId = entityId ?? Guid.NewGuid(),
            ServiceName = serviceName,
            UserEmail = "test@example.com",
            OrganizationId = organizationId ?? Guid.NewGuid(),
            WorkspaceId = Guid.NewGuid(),
            Details = """{"before":null,"after":{"name":"Acme"}}""",
            IpAddress = "192.168.1.1",
            UserAgent = "TestAgent/1.0",
            CorrelationId = Guid.NewGuid().ToString()
        };
    }

    [Fact]
    public async Task CreateAuditEntry_WithAllFields_ReturnsCreated()
    {
        // Arrange
        var request = CreateAuditEntryRequest();

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/audit", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var entry = await response.ReadAsAsync<AuditEntryDto>();
        entry.Id.Should().NotBeEmpty();
        entry.Action.Should().Be("organization.created");
        entry.EntityType.Should().Be("Organization");
        entry.ServiceName.Should().Be("Directory");
        entry.UserEmail.Should().Be("test@example.com");
        entry.IpAddress.Should().Be("192.168.1.1");
        entry.Timestamp.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(10));
    }

    [Fact]
    public async Task CreateAuditEntry_WithOnlyRequiredFields_ReturnsCreated()
    {
        // Arrange
        var request = new
        {
            UserId = Guid.NewGuid(),
            Action = "user.login",
            EntityType = "User",
            EntityId = Guid.NewGuid(),
            ServiceName = "Authentication"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/audit", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var entry = await response.ReadAsAsync<AuditEntryDto>();
        entry.Action.Should().Be("user.login");
        entry.ServiceName.Should().Be("Authentication");
        entry.OrganizationId.Should().BeNull();
    }

    [Fact]
    public async Task CreateAuditEntry_WithEmptyAction_ReturnsBadRequest()
    {
        // Arrange
        var request = new
        {
            UserId = Guid.NewGuid(),
            Action = "",
            EntityType = "Organization",
            EntityId = Guid.NewGuid(),
            ServiceName = "Directory"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/audit", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateAuditEntry_WithEmptyEntityType_ReturnsBadRequest()
    {
        // Arrange
        var request = new
        {
            UserId = Guid.NewGuid(),
            Action = "organization.created",
            EntityType = "",
            EntityId = Guid.NewGuid(),
            ServiceName = "Directory"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/audit", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateAuditEntry_WithEmptyServiceName_ReturnsBadRequest()
    {
        // Arrange
        var request = new
        {
            UserId = Guid.NewGuid(),
            Action = "organization.created",
            EntityType = "Organization",
            EntityId = Guid.NewGuid(),
            ServiceName = ""
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/audit", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAuditEntry_WithExistingId_ReturnsEntry()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/v1/audit", CreateAuditEntryRequest());
        var created = await createResponse.ReadAsAsync<AuditEntryDto>();

        // Act
        var response = await _client.GetAsync($"/api/v1/audit/{created.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var entry = await response.ReadAsAsync<AuditEntryDto>();
        entry.Id.Should().Be(created.Id);
        entry.Action.Should().Be(created.Action);
    }

    [Fact]
    public async Task GetAuditEntry_WithNonExistentId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/v1/audit/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAuditEntries_ReturnsPagedResults()
    {
        // Arrange
        await _client.PostAsJsonAsync("/api/v1/audit", CreateAuditEntryRequest());
        await _client.PostAsJsonAsync("/api/v1/audit", CreateAuditEntryRequest());

        // Act
        var response = await _client.GetAsync("/api/v1/audit?skip=0&take=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await response.ReadAsAsync<PagedAuditResponse>();
        paged.Items.Should().NotBeEmpty();
        paged.TotalCount.Should().BeGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task GetAuditEntries_FilterByOrganizationId_ReturnsMatchingEntries()
    {
        // Arrange
        var orgId = Guid.NewGuid();
        await _client.PostAsJsonAsync("/api/v1/audit", CreateAuditEntryRequest(organizationId: orgId));
        await _client.PostAsJsonAsync("/api/v1/audit", CreateAuditEntryRequest(organizationId: orgId));
        await _client.PostAsJsonAsync("/api/v1/audit", CreateAuditEntryRequest()); // different org

        // Act
        var response = await _client.GetAsync($"/api/v1/audit?organizationId={orgId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await response.ReadAsAsync<PagedAuditResponse>();
        paged.Items.Should().HaveCountGreaterThanOrEqualTo(2);
        paged.Items.Should().AllSatisfy(e => e.OrganizationId.Should().Be(orgId));
    }

    [Fact]
    public async Task GetAuditEntries_FilterByUserId_ReturnsMatchingEntries()
    {
        // Arrange
        var userId = Guid.NewGuid();
        await _client.PostAsJsonAsync("/api/v1/audit", CreateAuditEntryRequest(userId: userId));

        // Act
        var response = await _client.GetAsync($"/api/v1/audit?userId={userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await response.ReadAsAsync<PagedAuditResponse>();
        paged.Items.Should().AllSatisfy(e => e.UserId.Should().Be(userId));
    }

    [Fact]
    public async Task GetAuditEntries_FilterByEntityType_ReturnsMatchingEntries()
    {
        // Arrange
        await _client.PostAsJsonAsync("/api/v1/audit", CreateAuditEntryRequest(entityType: "Workspace"));

        // Act
        var response = await _client.GetAsync("/api/v1/audit?entityType=Workspace");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await response.ReadAsAsync<PagedAuditResponse>();
        paged.Items.Should().AllSatisfy(e => e.EntityType.Should().Be("Workspace"));
    }

    [Fact]
    public async Task GetAuditEntries_FilterByServiceName_ReturnsMatchingEntries()
    {
        // Arrange
        await _client.PostAsJsonAsync("/api/v1/audit",
            CreateAuditEntryRequest(serviceName: "AccessControl"));

        // Act
        var response = await _client.GetAsync("/api/v1/audit?serviceName=AccessControl");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var paged = await response.ReadAsAsync<PagedAuditResponse>();
        paged.Items.Should().AllSatisfy(e => e.ServiceName.Should().Be("AccessControl"));
    }

    [Fact]
    public async Task GetEntityHistory_ReturnsMatchingEntries()
    {
        // Arrange
        var entityId = Guid.NewGuid();
        await _client.PostAsJsonAsync("/api/v1/audit",
            CreateAuditEntryRequest(entityId: entityId, action: "organization.created"));
        await _client.PostAsJsonAsync("/api/v1/audit",
            CreateAuditEntryRequest(entityId: entityId, action: "organization.updated"));

        // Act
        var response = await _client.GetAsync($"/api/v1/audit/entity/Organization/{entityId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var entries = await response.ReadAsAsync<List<AuditEntryDto>>();
        entries.Should().HaveCountGreaterThanOrEqualTo(2);
        entries.Should().AllSatisfy(e => e.EntityId.Should().Be(entityId));
    }

    [Fact]
    public async Task PutAuditEntry_ReturnsMethodNotAllowed()
    {
        // Arrange - audit entries are immutable, PUT should not exist
        var createResponse = await _client.PostAsJsonAsync("/api/v1/audit", CreateAuditEntryRequest());
        var created = await createResponse.ReadAsAsync<AuditEntryDto>();

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/audit/{created.Id}", new { });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }

    [Fact]
    public async Task DeleteAuditEntry_ReturnsMethodNotAllowed()
    {
        // Arrange - audit entries are immutable, DELETE should not exist
        var createResponse = await _client.PostAsJsonAsync("/api/v1/audit", CreateAuditEntryRequest());
        var created = await createResponse.ReadAsAsync<AuditEntryDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/v1/audit/{created.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.MethodNotAllowed);
    }
}

file record AuditEntryDto(
    Guid Id,
    DateTime Timestamp,
    Guid UserId,
    string? UserEmail,
    string Action,
    string EntityType,
    Guid EntityId,
    Guid? OrganizationId,
    Guid? WorkspaceId,
    string? Details,
    string? IpAddress,
    string? UserAgent,
    string ServiceName,
    string? CorrelationId);

file record PagedAuditResponse(
    List<AuditEntryDto> Items,
    int TotalCount,
    int Skip,
    int Take);
