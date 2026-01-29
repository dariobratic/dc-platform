using Directory.API.DTOs;
using Directory.Application.Commands.Workspaces;
using Directory.Application.Queries.Workspaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Directory.API.Controllers;

[ApiController]
[Route("api/v1")]
public class WorkspacesController : ControllerBase
{
    private readonly ISender _sender;

    public WorkspacesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost("organizations/{orgId:guid}/workspaces")]
    [ProducesResponseType(typeof(WorkspaceResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(Guid orgId, CreateWorkspaceRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateWorkspaceCommand(orgId, request.Name, request.Slug);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("organizations/{orgId:guid}/workspaces")]
    [ProducesResponseType(typeof(IReadOnlyList<WorkspaceResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetByOrganization(Guid orgId, CancellationToken cancellationToken)
    {
        var query = new GetWorkspacesByOrganizationQuery(orgId);
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("workspaces/{id:guid}")]
    [ProducesResponseType(typeof(WorkspaceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetWorkspaceByIdQuery(id);
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPut("workspaces/{id:guid}")]
    [ProducesResponseType(typeof(WorkspaceResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, UpdateWorkspaceRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateWorkspaceCommand(id, request.Name);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("workspaces/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteWorkspaceCommand(id);
        await _sender.Send(command, cancellationToken);
        return NoContent();
    }
}
