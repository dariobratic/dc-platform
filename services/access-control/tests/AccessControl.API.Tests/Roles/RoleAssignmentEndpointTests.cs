using System.Net;
using System.Net.Http.Json;
using AccessControl.API.Tests.Extensions;
using AccessControl.API.Tests.Fixtures;
using FluentAssertions;

namespace AccessControl.API.Tests.Roles;

[Collection("Integration")]
[Trait("Category", "Integration")]
public class RoleAssignmentEndpointTests
{
    private readonly HttpClient _client;

    public RoleAssignmentEndpointTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Client;
    }

    private async Task<RoleDto> CreateTestRole()
    {
        var scopeId = Guid.NewGuid();
        var request = new
        {
            Name = $"Assignment Role {Guid.NewGuid():N}"[..30],
            Description = "Role for assignment tests",
            ScopeId = scopeId,
            ScopeType = 0,
            Permissions = new List<string> { "document:read" }
        };

        var response = await _client.PostAsJsonAsync("/api/v1/roles", request);
        return await response.ReadAsAsync<RoleDto>();
    }

    [Fact]
    public async Task AssignRole_WithValidData_ReturnsCreated()
    {
        // Arrange
        var role = await CreateTestRole();
        var request = new
        {
            UserId = Guid.NewGuid(),
            ScopeId = role.ScopeId,
            ScopeType = 0,
            AssignedBy = Guid.NewGuid()
        };

        // Act
        var response = await _client.PostAsJsonAsync($"/api/v1/roles/{role.Id}/assignments", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var assignment = await response.ReadAsAsync<RoleAssignmentDto>();
        assignment.RoleId.Should().Be(role.Id);
        assignment.UserId.Should().Be(request.UserId);
    }

    [Fact]
    public async Task AssignRole_DuplicateAssignment_ReturnsConflict()
    {
        // Arrange
        var role = await CreateTestRole();
        var userId = Guid.NewGuid();
        var request = new
        {
            UserId = userId,
            ScopeId = role.ScopeId,
            ScopeType = 0,
            AssignedBy = Guid.NewGuid()
        };

        await _client.PostAsJsonAsync($"/api/v1/roles/{role.Id}/assignments", request);

        // Act - duplicate assignment
        var response = await _client.PostAsJsonAsync($"/api/v1/roles/{role.Id}/assignments", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task RevokeRole_WithValidData_ReturnsNoContent()
    {
        // Arrange
        var role = await CreateTestRole();
        var userId = Guid.NewGuid();
        var assignRequest = new
        {
            UserId = userId,
            ScopeId = role.ScopeId,
            ScopeType = 0,
            AssignedBy = Guid.NewGuid()
        };

        await _client.PostAsJsonAsync($"/api/v1/roles/{role.Id}/assignments", assignRequest);

        var revokeRequest = new
        {
            UserId = userId,
            ScopeId = role.ScopeId
        };

        // Act
        var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/roles/{role.Id}/assignments")
        {
            Content = JsonContent.Create(revokeRequest)
        };
        var response = await _client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}

internal record RoleDto(
    Guid Id,
    string Name,
    string? Description,
    Guid ScopeId,
    string ScopeType,
    bool IsSystem,
    List<string> Permissions,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

internal record RoleAssignmentDto(
    Guid Id,
    Guid RoleId,
    string RoleName,
    Guid UserId,
    Guid ScopeId,
    string ScopeType,
    DateTime AssignedAt,
    Guid AssignedBy);
