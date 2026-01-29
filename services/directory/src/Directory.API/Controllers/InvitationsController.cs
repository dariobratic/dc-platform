using Directory.API.DTOs;
using Directory.Application.Commands.Invitations;
using Directory.Application.Queries.Invitations;
using Directory.Application.Queries.Memberships;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Directory.API.Controllers;

[ApiController]
[Route("api/v1")]
public class InvitationsController : ControllerBase
{
    private readonly ISender _sender;

    public InvitationsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("workspaces/{workspaceId:guid}/invitations")]
    [ProducesResponseType(typeof(InvitationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Create(Guid workspaceId, CreateInvitationRequest request, CancellationToken cancellationToken)
    {
        // TODO: Get InvitedBy from authenticated user context
        var invitedBy = Guid.Empty; // Placeholder - should come from authentication context

        var command = new CreateInvitationCommand(workspaceId, request.Email, request.Role, invitedBy);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetByToken), new { token = result.Token }, result);
    }

    [HttpGet("invitations/{token}")]
    [ProducesResponseType(typeof(InvitationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByToken(string token, CancellationToken cancellationToken)
    {
        var query = new GetInvitationByTokenQuery(token);
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost("invitations/{token}/accept")]
    [ProducesResponseType(typeof(MembershipResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Accept(string token, AcceptInvitationRequest request, CancellationToken cancellationToken)
    {
        var command = new AcceptInvitationCommand(token, request.UserId);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("invitations/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Revoke(Guid id, CancellationToken cancellationToken)
    {
        var command = new RevokeInvitationCommand(id);
        await _sender.Send(command, cancellationToken);
        return NoContent();
    }
}
