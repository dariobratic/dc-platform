using System.Net;
using System.Net.Http.Json;
using AccessControl.API.Tests.Extensions;
using AccessControl.API.Tests.Fixtures;
using FluentAssertions;

namespace AccessControl.API.Tests.Permissions;

[Collection("Integration")]
[Trait("Category", "Integration")]
public class PermissionEndpointTests
{
    private readonly HttpClient _client;

    public PermissionEndpointTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Client;
    }

    private async Task<(PermissionRoleDto Role, Guid UserId)> CreateRoleWithAssignment(List<string> permissions)
    {
        var scopeId = Guid.NewGuid();
        var roleRequest = new
        {
            Name = $"Perm Role {Guid.NewGuid():N}"[..30],
            Description = "Permission test role",
            ScopeId = scopeId,
            ScopeType = 0,
            Permissions = permissions
        };

        var roleResponse = await _client.PostAsJsonAsync("/api/v1/roles", roleRequest);
        var role = await roleResponse.ReadAsAsync<PermissionRoleDto>();

        var userId = Guid.NewGuid();
        var assignRequest = new
        {
            UserId = userId,
            ScopeId = scopeId,
            ScopeType = 0,
            AssignedBy = Guid.NewGuid()
        };
        await _client.PostAsJsonAsync($"/api/v1/roles/{role.Id}/assignments", assignRequest);

        return (role, userId);
    }

    [Fact]
    public async Task CheckPermission_WhenUserHasPermission_ReturnsTrue()
    {
        // Arrange
        var (role, userId) = await CreateRoleWithAssignment(["document:read", "document:write"]);

        // Act
        var response = await _client.GetAsync(
            $"/api/v1/permissions/check?userId={userId}&scopeId={role.ScopeId}&permission=document:read");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.ReadAsAsync<PermissionCheckDto>();
        result.HasPermission.Should().BeTrue();
        result.UserId.Should().Be(userId);
        result.Permission.Should().Be("document:read");
    }

    [Fact]
    public async Task CheckPermission_WhenUserLacksPermission_ReturnsFalse()
    {
        // Arrange
        var (role, userId) = await CreateRoleWithAssignment(["document:read"]);

        // Act
        var response = await _client.GetAsync(
            $"/api/v1/permissions/check?userId={userId}&scopeId={role.ScopeId}&permission=document:delete");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.ReadAsAsync<PermissionCheckDto>();
        result.HasPermission.Should().BeFalse();
    }

    [Fact]
    public async Task GetUserPermissions_ReturnsAllPermissionsInScope()
    {
        // Arrange
        var (role, userId) = await CreateRoleWithAssignment(["document:read", "document:write"]);

        // Act
        var response = await _client.GetAsync(
            $"/api/v1/users/{userId}/permissions?scopeId={role.ScopeId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var permissions = await response.ReadAsAsync<List<string>>();
        permissions.Should().Contain("document:read");
        permissions.Should().Contain("document:write");
    }

    [Fact]
    public async Task GetUserPermissions_WithNoAssignments_ReturnsEmpty()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var scopeId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync(
            $"/api/v1/users/{userId}/permissions?scopeId={scopeId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var permissions = await response.ReadAsAsync<List<string>>();
        permissions.Should().BeEmpty();
    }
}

internal record PermissionRoleDto(
    Guid Id,
    string Name,
    string? Description,
    Guid ScopeId,
    string ScopeType,
    bool IsSystem,
    List<string> Permissions,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

internal record PermissionCheckDto(
    bool HasPermission,
    Guid UserId,
    Guid ScopeId,
    string Permission);
