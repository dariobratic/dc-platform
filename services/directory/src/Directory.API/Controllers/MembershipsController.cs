using Directory.API.DTOs;
using Directory.Application.Commands.Memberships;
using Directory.Application.Queries.Memberships;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Directory.API.Controllers;

[ApiController]
[Route("api/v1")]
public class MembershipsController : ControllerBase
{
    private readonly ISender _sender;

    public MembershipsController(ISender sender)
    {
        _sender = sender;
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
}
