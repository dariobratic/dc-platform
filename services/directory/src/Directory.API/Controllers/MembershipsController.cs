using System.Text;
using System.Text.Json;
using Directory.API.DTOs;
using Directory.Application.Commands.Memberships;
using Directory.Application.Queries.Memberships;
using Directory.Application.Queries.Organizations;
using Directory.Application.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Directory.API.Controllers;

[ApiController]
[Route("api/v1")]
public class MembershipsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly IOrganizationRepository _organizationRepository;

    public MembershipsController(ISender sender, IOrganizationRepository organizationRepository)
    {
        _sender = sender;
        _organizationRepository = organizationRepository;
    }

    [HttpPost("workspaces/{workspaceId:guid}/members")]
    [ProducesResponseType(typeof(MembershipResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddMember(Guid workspaceId, AddMemberRequest request, CancellationToken cancellationToken)
    {
        var command = new AddMemberCommand(workspaceId, request.UserId, request.Role);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetWorkspaceMembers), new { workspaceId }, result);
    }

    [HttpGet("workspaces/{workspaceId:guid}/members")]
    [ProducesResponseType(typeof(IReadOnlyList<MembershipResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetWorkspaceMembers(Guid workspaceId, CancellationToken cancellationToken)
    {
        var query = new GetWorkspaceMembersQuery(workspaceId);
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPut("workspaces/{workspaceId:guid}/members/{userId:guid}")]
    [ProducesResponseType(typeof(MembershipResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeMemberRole(Guid workspaceId, Guid userId, ChangeMemberRoleRequest request, CancellationToken cancellationToken)
    {
        var command = new ChangeMemberRoleCommand(workspaceId, userId, request.Role);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("workspaces/{workspaceId:guid}/members/{userId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveMember(Guid workspaceId, Guid userId, CancellationToken cancellationToken)
    {
        var command = new RemoveMemberCommand(workspaceId, userId);
        await _sender.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpGet("users/{userId:guid}/memberships")]
    [ProducesResponseType(typeof(IReadOnlyList<MembershipResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetUserMemberships(Guid userId, CancellationToken cancellationToken)
    {
        var query = new GetUserMembershipsQuery(userId);
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("users/me/organizations")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrentUserOrganizations(CancellationToken cancellationToken)
    {
        var organizationId = GetOrganizationIdFromToken();

        if (organizationId is null)
            return Ok(new { organizations = Array.Empty<object>() });

        var organization = await _organizationRepository.GetByIdAsync(organizationId.Value, cancellationToken);

        if (organization is null)
            return Ok(new { organizations = Array.Empty<object>() });

        var response = OrganizationResponse.FromEntity(organization);
        return Ok(new { organizations = new[] { response } });
    }

    private Guid? GetOrganizationIdFromToken()
    {
        var authHeader = Request.Headers.Authorization.ToString();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            return null;

        var token = authHeader["Bearer ".Length..];
        var parts = token.Split('.');
        if (parts.Length < 2)
            return null;

        var payload = parts[1];
        payload = payload.Replace('-', '+').Replace('_', '/');
        switch (payload.Length % 4)
        {
            case 2: payload += "=="; break;
            case 3: payload += "="; break;
        }

        try
        {
            var bytes = Convert.FromBase64String(payload);
            var json = Encoding.UTF8.GetString(bytes);
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("organization_id", out var orgIdProp))
            {
                if (Guid.TryParse(orgIdProp.GetString(), out var orgId))
                    return orgId;
            }
        }
        catch
        {
            // Invalid token format — return null
        }

        return null;
    }
}
