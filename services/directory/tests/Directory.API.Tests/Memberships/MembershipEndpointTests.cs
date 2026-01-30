using System.Net;
using System.Net.Http.Json;
using Directory.API.Tests.Extensions;
using Directory.API.Tests.Fixtures;
using FluentAssertions;

namespace Directory.API.Tests.Memberships;

[Collection("Integration")]
[Trait("Category", "Integration")]
public class MembershipEndpointTests
{
    private readonly HttpClient _client;

    public MembershipEndpointTests(IntegrationTestFixture fixture)
    {
        _client = fixture.Client;
    }

    private static string UniqueSlug() => $"test-{Guid.NewGuid():N}"[..26];

    [Fact]
    public async Task AddMember_WithValidData_ReturnsCreatedWithMembership()
    {
        // Arrange
        var orgSlug = UniqueSlug();
        var orgResponse = await _client.PostAsJsonAsync("/api/v1/organizations",
            new { Name = "Test Org", Slug = orgSlug });
        var org = await orgResponse.ReadAsAsync<IdDto>();

        var wsSlug = UniqueSlug();
        var wsResponse = await _client.PostAsJsonAsync(
            $"/api/v1/organizations/{org.Id}/workspaces",
            new { Name = "Test Workspace", Slug = wsSlug });
        var ws = await wsResponse.ReadAsAsync<IdDto>();

        var userId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/workspaces/{ws.Id}/members",
            new { UserId = userId, Role = "Member" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var membership = await response.ReadAsAsync<MembershipDto>();
        membership.WorkspaceId.Should().Be(ws.Id);
        membership.UserId.Should().Be(userId);
        membership.Role.Should().Be("Member");
    }

    [Fact]
    public async Task AddMember_WithOwnerRole_ReturnsOwnerMembership()
    {
        // Arrange
        var orgSlug = UniqueSlug();
        var orgResponse = await _client.PostAsJsonAsync("/api/v1/organizations",
            new { Name = "Test Org", Slug = orgSlug });
        var org = await orgResponse.ReadAsAsync<IdDto>();

        var wsSlug = UniqueSlug();
        var wsResponse = await _client.PostAsJsonAsync(
            $"/api/v1/organizations/{org.Id}/workspaces",
            new { Name = "Test Workspace", Slug = wsSlug });
        var ws = await wsResponse.ReadAsAsync<IdDto>();

        var userId = Guid.NewGuid();

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/workspaces/{ws.Id}/members",
            new { UserId = userId, Role = "Owner" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var membership = await response.ReadAsAsync<MembershipDto>();
        membership.Role.Should().Be("Owner");
    }

    [Fact]
    public async Task AddMember_DuplicateUser_ReturnsConflict()
    {
        // Arrange
        var orgSlug = UniqueSlug();
        var orgResponse = await _client.PostAsJsonAsync("/api/v1/organizations",
            new { Name = "Test Org", Slug = orgSlug });
        var org = await orgResponse.ReadAsAsync<IdDto>();

        var wsSlug = UniqueSlug();
        var wsResponse = await _client.PostAsJsonAsync(
            $"/api/v1/organizations/{org.Id}/workspaces",
            new { Name = "Test Workspace", Slug = wsSlug });
        var ws = await wsResponse.ReadAsAsync<IdDto>();

        var userId = Guid.NewGuid();
        await _client.PostAsJsonAsync($"/api/v1/workspaces/{ws.Id}/members",
            new { UserId = userId, Role = "Member" });

        // Act
        var response = await _client.PostAsJsonAsync(
            $"/api/v1/workspaces/{ws.Id}/members",
            new { UserId = userId, Role = "Admin" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task GetMembers_ReturnsAllWorkspaceMembers()
    {
        // Arrange
        var orgSlug = UniqueSlug();
        var orgResponse = await _client.PostAsJsonAsync("/api/v1/organizations",
            new { Name = "Test Org", Slug = orgSlug });
        var org = await orgResponse.ReadAsAsync<IdDto>();

        var wsSlug = UniqueSlug();
        var wsResponse = await _client.PostAsJsonAsync(
            $"/api/v1/organizations/{org.Id}/workspaces",
            new { Name = "Test Workspace", Slug = wsSlug });
        var ws = await wsResponse.ReadAsAsync<IdDto>();

        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();
        await _client.PostAsJsonAsync($"/api/v1/workspaces/{ws.Id}/members",
            new { UserId = user1, Role = "Admin" });
        await _client.PostAsJsonAsync($"/api/v1/workspaces/{ws.Id}/members",
            new { UserId = user2, Role = "Viewer" });

        // Act
        var response = await _client.GetAsync($"/api/v1/workspaces/{ws.Id}/members");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var members = await response.ReadAsAsync<List<MembershipDto>>();
        members.Should().HaveCount(2);
        members.Should().Contain(m => m.UserId == user1 && m.Role == "Admin");
        members.Should().Contain(m => m.UserId == user2 && m.Role == "Viewer");
    }

    [Fact]
    public async Task ChangeMemberRole_WithValidData_ReturnsUpdatedMembership()
    {
        // Arrange
        var orgSlug = UniqueSlug();
        var orgResponse = await _client.PostAsJsonAsync("/api/v1/organizations",
            new { Name = "Test Org", Slug = orgSlug });
        var org = await orgResponse.ReadAsAsync<IdDto>();

        var wsSlug = UniqueSlug();
        var wsResponse = await _client.PostAsJsonAsync(
            $"/api/v1/organizations/{org.Id}/workspaces",
            new { Name = "Test Workspace", Slug = wsSlug });
        var ws = await wsResponse.ReadAsAsync<IdDto>();

        var userId = Guid.NewGuid();
        await _client.PostAsJsonAsync($"/api/v1/workspaces/{ws.Id}/members",
            new { UserId = userId, Role = "Member" });

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/v1/workspaces/{ws.Id}/members/{userId}",
            new { Role = "Admin" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.ReadAsAsync<MembershipDto>();
        updated.Role.Should().Be("Admin");
    }

    [Fact]
    public async Task ChangeMemberRole_ToOwner_ReturnsOwnerRole()
    {
        // Arrange
        var orgSlug = UniqueSlug();
        var orgResponse = await _client.PostAsJsonAsync("/api/v1/organizations",
            new { Name = "Test Org", Slug = orgSlug });
        var org = await orgResponse.ReadAsAsync<IdDto>();

        var wsSlug = UniqueSlug();
        var wsResponse = await _client.PostAsJsonAsync(
            $"/api/v1/organizations/{org.Id}/workspaces",
            new { Name = "Test Workspace", Slug = wsSlug });
        var ws = await wsResponse.ReadAsAsync<IdDto>();

        var userId = Guid.NewGuid();
        await _client.PostAsJsonAsync($"/api/v1/workspaces/{ws.Id}/members",
            new { UserId = userId, Role = "Viewer" });

        // Act
        var response = await _client.PutAsJsonAsync(
            $"/api/v1/workspaces/{ws.Id}/members/{userId}",
            new { Role = "Owner" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updated = await response.ReadAsAsync<MembershipDto>();
        updated.Role.Should().Be("Owner");
    }

    [Fact]
    public async Task RemoveMember_ThatExists_ReturnsNoContent()
    {
        // Arrange
        var orgSlug = UniqueSlug();
        var orgResponse = await _client.PostAsJsonAsync("/api/v1/organizations",
            new { Name = "Test Org", Slug = orgSlug });
        var org = await orgResponse.ReadAsAsync<IdDto>();

        var wsSlug = UniqueSlug();
        var wsResponse = await _client.PostAsJsonAsync(
            $"/api/v1/organizations/{org.Id}/workspaces",
            new { Name = "Test Workspace", Slug = wsSlug });
        var ws = await wsResponse.ReadAsAsync<IdDto>();

        var userId = Guid.NewGuid();
        await _client.PostAsJsonAsync($"/api/v1/workspaces/{ws.Id}/members",
            new { UserId = userId, Role = "Member" });

        // Act
        var response = await _client.DeleteAsync(
            $"/api/v1/workspaces/{ws.Id}/members/{userId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task RemoveMember_ThenGetMembers_MemberIsGone()
    {
        // Arrange
        var orgSlug = UniqueSlug();
        var orgResponse = await _client.PostAsJsonAsync("/api/v1/organizations",
            new { Name = "Test Org", Slug = orgSlug });
        var org = await orgResponse.ReadAsAsync<IdDto>();

        var wsSlug = UniqueSlug();
        var wsResponse = await _client.PostAsJsonAsync(
            $"/api/v1/organizations/{org.Id}/workspaces",
            new { Name = "Test Workspace", Slug = wsSlug });
        var ws = await wsResponse.ReadAsAsync<IdDto>();

        var userId = Guid.NewGuid();
        await _client.PostAsJsonAsync($"/api/v1/workspaces/{ws.Id}/members",
            new { UserId = userId, Role = "Member" });
        await _client.DeleteAsync($"/api/v1/workspaces/{ws.Id}/members/{userId}");

        // Act
        var response = await _client.GetAsync($"/api/v1/workspaces/{ws.Id}/members");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var members = await response.ReadAsAsync<List<MembershipDto>>();
        members.Should().NotContain(m => m.UserId == userId);
    }

    [Fact]
    public async Task GetUserMemberships_ReturnsAllMembershipsForUser()
    {
        // Arrange
        var orgSlug1 = UniqueSlug();
        var orgResponse1 = await _client.PostAsJsonAsync("/api/v1/organizations",
            new { Name = "Test Org 1", Slug = orgSlug1 });
        var org1 = await orgResponse1.ReadAsAsync<IdDto>();

        var wsSlug1 = UniqueSlug();
        var wsResponse1 = await _client.PostAsJsonAsync(
            $"/api/v1/organizations/{org1.Id}/workspaces",
            new { Name = "Test Workspace 1", Slug = wsSlug1 });
        var ws1 = await wsResponse1.ReadAsAsync<IdDto>();

        var orgSlug2 = UniqueSlug();
        var orgResponse2 = await _client.PostAsJsonAsync("/api/v1/organizations",
            new { Name = "Test Org 2", Slug = orgSlug2 });
        var org2 = await orgResponse2.ReadAsAsync<IdDto>();

        var wsSlug2 = UniqueSlug();
        var wsResponse2 = await _client.PostAsJsonAsync(
            $"/api/v1/organizations/{org2.Id}/workspaces",
            new { Name = "Test Workspace 2", Slug = wsSlug2 });
        var ws2 = await wsResponse2.ReadAsAsync<IdDto>();

        var userId = Guid.NewGuid();
        await _client.PostAsJsonAsync($"/api/v1/workspaces/{ws1.Id}/members",
            new { UserId = userId, Role = "Admin" });
        await _client.PostAsJsonAsync($"/api/v1/workspaces/{ws2.Id}/members",
            new { UserId = userId, Role = "Viewer" });

        // Act
        var response = await _client.GetAsync($"/api/v1/users/{userId}/memberships");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var memberships = await response.ReadAsAsync<List<MembershipDto>>();
        memberships.Should().HaveCount(2);
        memberships.Should().Contain(m => m.WorkspaceId == ws1.Id && m.Role == "Admin");
        memberships.Should().Contain(m => m.WorkspaceId == ws2.Id && m.Role == "Viewer");
    }

    [Fact]
    public async Task MembershipFlow_AddChangeRemove_WorksEndToEnd()
    {
        // Arrange
        var orgSlug = UniqueSlug();
        var orgResponse = await _client.PostAsJsonAsync("/api/v1/organizations",
            new { Name = "Test Org", Slug = orgSlug });
        var org = await orgResponse.ReadAsAsync<IdDto>();

        var wsSlug = UniqueSlug();
        var wsResponse = await _client.PostAsJsonAsync(
            $"/api/v1/organizations/{org.Id}/workspaces",
            new { Name = "Test Workspace", Slug = wsSlug });
        var ws = await wsResponse.ReadAsAsync<IdDto>();

        var userId = Guid.NewGuid();

        // Act 1: Add member as Viewer
        var addResponse = await _client.PostAsJsonAsync(
            $"/api/v1/workspaces/{ws.Id}/members",
            new { UserId = userId, Role = "Viewer" });
        addResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var added = await addResponse.ReadAsAsync<MembershipDto>();
        added.Role.Should().Be("Viewer");

        // Act 2: Promote to Admin
        var changeResponse = await _client.PutAsJsonAsync(
            $"/api/v1/workspaces/{ws.Id}/members/{userId}",
            new { Role = "Admin" });
        changeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var changed = await changeResponse.ReadAsAsync<MembershipDto>();
        changed.Role.Should().Be("Admin");

        // Act 3: Remove member
        var removeResponse = await _client.DeleteAsync(
            $"/api/v1/workspaces/{ws.Id}/members/{userId}");
        removeResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Verify: member is gone
        var membersResponse = await _client.GetAsync($"/api/v1/workspaces/{ws.Id}/members");
        var members = await membersResponse.ReadAsAsync<List<MembershipDto>>();
        members.Should().NotContain(m => m.UserId == userId);
    }
}

file record IdDto(Guid Id);

file record MembershipDto(Guid Id, Guid WorkspaceId, Guid UserId, string Role,
    DateTime JoinedAt, Guid? InvitedBy);
