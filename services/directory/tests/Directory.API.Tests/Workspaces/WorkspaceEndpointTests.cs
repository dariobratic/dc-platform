using System.Net;
using System.Net.Http.Json;
using Directory.API.Tests.Extensions;
using Directory.API.Tests.Fixtures;
using FluentAssertions;

namespace Directory.API.Tests.Workspaces;

[Collection("Integration")]
[Trait("Category", "Integration")]
public class WorkspaceEndpointTests
{
    private readonly HttpClient _client;

    public WorkspaceEndpointTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Client;
    }

    private static string UniqueSlug() => $"test-ws-{Guid.NewGuid():N}"[..28];

    [Fact]
    public async Task CreateWorkspace_WithValidData_ReturnsCreatedWithWorkspace()
    {
        // Arrange
        var orgSlug = $"org-{Guid.NewGuid():N}"[..28];
        var orgResponse = await _client.PostAsJsonAsync("/api/v1/organizations",
            new { Name = "Test Org", Slug = orgSlug });
        var org = await orgResponse.ReadAsAsync<OrganizationDto>();
        var slug = UniqueSlug();

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/organizations/{org.Id}/workspaces",
            new { Name = "Test Workspace", Slug = slug });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var ws = await response.ReadAsAsync<WorkspaceDto>();
        ws.Id.Should().NotBeEmpty();
        ws.OrganizationId.Should().Be(org.Id);
        ws.Name.Should().Be("Test Workspace");
        ws.Slug.Should().Be(slug);
        ws.Status.Should().Be("Active");
    }

    [Fact]
    public async Task CreateWorkspace_WithDuplicateSlugInSameOrg_ReturnsConflict()
    {
        // Arrange
        var orgSlug = $"org-{Guid.NewGuid():N}"[..28];
        var orgResponse = await _client.PostAsJsonAsync("/api/v1/organizations",
            new { Name = "Test Org", Slug = orgSlug });
        var org = await orgResponse.ReadAsAsync<OrganizationDto>();
        var slug = UniqueSlug();
        await _client.PostAsJsonAsync($"/api/v1/organizations/{org.Id}/workspaces",
            new { Name = "First WS", Slug = slug });

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/organizations/{org.Id}/workspaces",
            new { Name = "Second WS", Slug = slug });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateWorkspace_WithSameSlugInDifferentOrgs_BothSucceed()
    {
        // Arrange
        var orgSlug1 = $"org-{Guid.NewGuid():N}"[..28];
        var orgResponse1 = await _client.PostAsJsonAsync("/api/v1/organizations",
            new { Name = "Test Org 1", Slug = orgSlug1 });
        var org1 = await orgResponse1.ReadAsAsync<OrganizationDto>();

        var orgSlug2 = $"org-{Guid.NewGuid():N}"[..28];
        var orgResponse2 = await _client.PostAsJsonAsync("/api/v1/organizations",
            new { Name = "Test Org 2", Slug = orgSlug2 });
        var org2 = await orgResponse2.ReadAsAsync<OrganizationDto>();

        var slug = UniqueSlug();

        // Act
        var response1 = await _client.PostAsJsonAsync(
            $"/api/v1/organizations/{org1.Id}/workspaces",
            new { Name = "WS in Org 1", Slug = slug });
        var response2 = await _client.PostAsJsonAsync(
            $"/api/v1/organizations/{org2.Id}/workspaces",
            new { Name = "WS in Org 2", Slug = slug });

        // Assert
        response1.StatusCode.Should().Be(HttpStatusCode.Created);
        response2.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetWorkspace_WithExistingId_ReturnsWorkspace()
    {
        // Arrange
        var orgSlug = $"org-{Guid.NewGuid():N}"[..28];
        var orgResponse = await _client.PostAsJsonAsync("/api/v1/organizations",
            new { Name = "Test Org", Slug = orgSlug });
        var org = await orgResponse.ReadAsAsync<OrganizationDto>();

        var createResponse = await _client.PostAsJsonAsync(
            $"/api/v1/organizations/{org.Id}/workspaces",
            new { Name = "Get Test WS", Slug = UniqueSlug() });
        var created = await createResponse.ReadAsAsync<WorkspaceDto>();

        // Act
        var response = await _client.GetAsync($"/api/v1/workspaces/{created.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var ws = await response.ReadAsAsync<WorkspaceDto>();
        ws.Id.Should().Be(created.Id);
        ws.Name.Should().Be("Get Test WS");
    }

    [Fact]
    public async Task GetWorkspace_WithNonExistentId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/v1/workspaces/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ListWorkspaces_ForOrganization_ReturnsAllWorkspaces()
    {
        // Arrange
        var orgSlug = $"org-{Guid.NewGuid():N}"[..28];
        var orgResponse = await _client.PostAsJsonAsync("/api/v1/organizations",
            new { Name = "Test Org", Slug = orgSlug });
        var org = await orgResponse.ReadAsAsync<OrganizationDto>();

        await _client.PostAsJsonAsync($"/api/v1/organizations/{org.Id}/workspaces",
            new { Name = "WS Alpha", Slug = UniqueSlug() });
        await _client.PostAsJsonAsync($"/api/v1/organizations/{org.Id}/workspaces",
            new { Name = "WS Beta", Slug = UniqueSlug() });

        // Act
        var response = await _client.GetAsync($"/api/v1/organizations/{org.Id}/workspaces");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var workspaces = await response.ReadAsAsync<List<WorkspaceDto>>();
        workspaces.Should().HaveCount(2);
    }

    [Fact]
    public async Task UpdateWorkspace_WithValidData_ReturnsUpdatedWorkspace()
    {
        // Arrange
        var orgSlug = $"org-{Guid.NewGuid():N}"[..28];
        var orgResponse = await _client.PostAsJsonAsync("/api/v1/organizations",
            new { Name = "Test Org", Slug = orgSlug });
        var org = await orgResponse.ReadAsAsync<OrganizationDto>();

        var createResponse = await _client.PostAsJsonAsync(
            $"/api/v1/organizations/{org.Id}/workspaces",
            new { Name = "Original WS", Slug = UniqueSlug() });
        var created = await createResponse.ReadAsAsync<WorkspaceDto>();

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/workspaces/{created.Id}",
            new { Name = "Updated WS" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.ReadAsAsync<WorkspaceDto>();
        updated.Name.Should().Be("Updated WS");
        updated.UpdatedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task DeleteWorkspace_ThatExists_ReturnsNoContent()
    {
        // Arrange
        var orgSlug = $"org-{Guid.NewGuid():N}"[..28];
        var orgResponse = await _client.PostAsJsonAsync("/api/v1/organizations",
            new { Name = "Test Org", Slug = orgSlug });
        var org = await orgResponse.ReadAsAsync<OrganizationDto>();

        var createResponse = await _client.PostAsJsonAsync(
            $"/api/v1/organizations/{org.Id}/workspaces",
            new { Name = "To Delete WS", Slug = UniqueSlug() });
        var created = await createResponse.ReadAsAsync<WorkspaceDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/v1/workspaces/{created.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteWorkspace_ThenGet_ReturnsNotFound()
    {
        // Arrange
        var orgSlug = $"org-{Guid.NewGuid():N}"[..28];
        var orgResponse = await _client.PostAsJsonAsync("/api/v1/organizations",
            new { Name = "Test Org", Slug = orgSlug });
        var org = await orgResponse.ReadAsAsync<OrganizationDto>();

        var createResponse = await _client.PostAsJsonAsync(
            $"/api/v1/organizations/{org.Id}/workspaces",
            new { Name = "Delete Then Get WS", Slug = UniqueSlug() });
        var created = await createResponse.ReadAsAsync<WorkspaceDto>();
        await _client.DeleteAsync($"/api/v1/workspaces/{created.Id}");

        // Act
        var response = await _client.GetAsync($"/api/v1/workspaces/{created.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

file record OrganizationDto(Guid Id, string Name, string Slug, string Status,
    Dictionary<string, string> Settings, DateTime CreatedAt, DateTime? UpdatedAt);

file record WorkspaceDto(Guid Id, Guid OrganizationId, string Name, string Slug,
    string Status, DateTime CreatedAt, DateTime? UpdatedAt);
