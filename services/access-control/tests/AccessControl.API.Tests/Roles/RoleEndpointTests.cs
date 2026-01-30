using System.Net;
using System.Net.Http.Json;
using AccessControl.API.Tests.Extensions;
using AccessControl.API.Tests.Fixtures;
using FluentAssertions;

namespace AccessControl.API.Tests.Roles;

[Collection("Integration")]
[Trait("Category", "Integration")]
public class RoleEndpointTests
{
    private readonly HttpClient _client;

    public RoleEndpointTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Client;
    }

    private static object CreateRoleRequest(string? name = null, List<string>? permissions = null)
    {
        return new
        {
            Name = name ?? $"Test Role {Guid.NewGuid():N}"[..30],
            Description = "Test role description",
            ScopeId = Guid.NewGuid(),
            ScopeType = 0, // Organization
            Permissions = permissions ?? new List<string> { "document:read" }
        };
    }

    [Fact]
    public async Task CreateRole_WithValidData_ReturnsCreatedWithRole()
    {
        // Arrange
        var scopeId = Guid.NewGuid();
        var request = new
        {
            Name = "Admin Role",
            Description = "Full admin access",
            ScopeId = scopeId,
            ScopeType = 0,
            Permissions = new List<string> { "document:read", "document:write" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/roles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var role = await response.ReadAsAsync<RoleDto>();
        role.Id.Should().NotBeEmpty();
        role.Name.Should().Be("Admin Role");
        role.Description.Should().Be("Full admin access");
        role.ScopeId.Should().Be(scopeId);
        role.Permissions.Should().Contain("document:read");
        role.Permissions.Should().Contain("document:write");
    }

    [Fact]
    public async Task CreateRole_WithEmptyName_ReturnsBadRequest()
    {
        // Arrange
        var request = new
        {
            Name = "",
            Description = "Desc",
            ScopeId = Guid.NewGuid(),
            ScopeType = 0,
            Permissions = new List<string> { "document:read" }
        };

        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/roles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateRole_WithDuplicateNameInSameScope_ReturnsConflict()
    {
        // Arrange
        var scopeId = Guid.NewGuid();
        var request = new
        {
            Name = $"Unique Role {Guid.NewGuid():N}"[..30],
            Description = "First",
            ScopeId = scopeId,
            ScopeType = 0,
            Permissions = new List<string> { "document:read" }
        };

        await _client.PostAsJsonAsync("/api/v1/roles", request);

        // Act - same name, same scope
        var response = await _client.PostAsJsonAsync("/api/v1/roles", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetRole_WithExistingId_ReturnsRole()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/v1/roles", CreateRoleRequest());
        var created = await createResponse.ReadAsAsync<RoleDto>();

        // Act
        var response = await _client.GetAsync($"/api/v1/roles/{created.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var role = await response.ReadAsAsync<RoleDto>();
        role.Id.Should().Be(created.Id);
        role.Name.Should().Be(created.Name);
    }

    [Fact]
    public async Task GetRole_WithNonExistentId_ReturnsNotFound()
    {
        // Act
        var response = await _client.GetAsync($"/api/v1/roles/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetRolesByScope_ReturnsMatchingRoles()
    {
        // Arrange
        var scopeId = Guid.NewGuid();
        var request1 = new
        {
            Name = $"Scope Role 1 {Guid.NewGuid():N}"[..30],
            Description = "First",
            ScopeId = scopeId,
            ScopeType = 0,
            Permissions = new List<string> { "document:read" }
        };
        var request2 = new
        {
            Name = $"Scope Role 2 {Guid.NewGuid():N}"[..30],
            Description = "Second",
            ScopeId = scopeId,
            ScopeType = 0,
            Permissions = new List<string> { "document:write" }
        };

        await _client.PostAsJsonAsync("/api/v1/roles", request1);
        await _client.PostAsJsonAsync("/api/v1/roles", request2);

        // Act
        var response = await _client.GetAsync($"/api/v1/roles?scopeId={scopeId}&scopeType=Organization");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var roles = await response.ReadAsAsync<List<RoleDto>>();
        roles.Should().HaveCountGreaterThanOrEqualTo(2);
    }

    [Fact]
    public async Task UpdateRole_WithValidData_ReturnsUpdatedRole()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/v1/roles", CreateRoleRequest());
        var created = await createResponse.ReadAsAsync<RoleDto>();

        var updateRequest = new
        {
            Name = "Updated Role Name",
            Description = "Updated description",
            Permissions = new List<string> { "document:read", "document:delete" }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/roles/{created.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.ReadAsAsync<RoleDto>();
        updated.Name.Should().Be("Updated Role Name");
        updated.Description.Should().Be("Updated description");
        updated.Permissions.Should().Contain("document:delete");
    }

    [Fact]
    public async Task UpdateRole_WithNonExistentId_ReturnsNotFound()
    {
        // Arrange
        var updateRequest = new
        {
            Name = "Updated",
            Description = "Desc",
            Permissions = new List<string> { "document:read" }
        };

        // Act
        var response = await _client.PutAsJsonAsync($"/api/v1/roles/{Guid.NewGuid()}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteRole_ThatExists_ReturnsNoContent()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/v1/roles", CreateRoleRequest());
        var created = await createResponse.ReadAsAsync<RoleDto>();

        // Act
        var response = await _client.DeleteAsync($"/api/v1/roles/{created.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DeleteRole_ThenGet_ReturnsNotFound()
    {
        // Arrange
        var createResponse = await _client.PostAsJsonAsync("/api/v1/roles", CreateRoleRequest());
        var created = await createResponse.ReadAsAsync<RoleDto>();

        await _client.DeleteAsync($"/api/v1/roles/{created.Id}");

        // Act
        var response = await _client.GetAsync($"/api/v1/roles/{created.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteRole_WithNonExistentId_ReturnsNotFound()
    {
        // Act
        var response = await _client.DeleteAsync($"/api/v1/roles/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}

file record RoleDto(
    Guid Id,
    string Name,
    string? Description,
    Guid ScopeId,
    string ScopeType,
    bool IsSystem,
    List<string> Permissions,
    DateTime CreatedAt,
    DateTime? UpdatedAt);
