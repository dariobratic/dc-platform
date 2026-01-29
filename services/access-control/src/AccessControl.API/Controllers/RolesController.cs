using AccessControl.API.DTOs;
using AccessControl.Application.Commands.Roles.CreateRole;
using AccessControl.Application.Commands.Roles.DeleteRole;
using AccessControl.Application.Commands.Roles.UpdateRole;
using AccessControl.Application.Commands.RoleAssignments.AssignRole;
using AccessControl.Application.Commands.RoleAssignments.RevokeRole;
using AccessControl.Application.Queries.Roles.GetRoleById;
using AccessControl.Application.Queries.Roles.GetRolesByScope;
using AccessControl.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AccessControl.API.Controllers;

[ApiController]
[Route("api/v1/roles")]
public class RolesController : ControllerBase
{
    private readonly ISender _sender;

    public RolesController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateRoleRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateRoleCommand(
            request.Name,
            request.Description,
            request.ScopeId,
            request.ScopeType,
            request.Permissions);

        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetRoleByIdQuery(id);
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetByScope(
        [FromQuery] Guid scopeId,
        [FromQuery] ScopeType scopeType,
        CancellationToken cancellationToken)
    {
        var query = new GetRolesByScopeQuery(scopeId, scopeType);
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, UpdateRoleRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateRoleCommand(
            id,
            request.Name,
            request.Description,
            request.Permissions);

        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteRoleCommand(id);
        await _sender.Send(command, cancellationToken);
        return NoContent();
    }

    [HttpPost("{roleId:guid}/assignments")]
    public async Task<IActionResult> AssignRole(Guid roleId, AssignRoleRequest request, CancellationToken cancellationToken)
    {
        var command = new AssignRoleCommand(
            roleId,
            request.UserId,
            request.ScopeId,
            request.ScopeType,
            request.AssignedBy);

        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = roleId }, result);
    }

    [HttpDelete("{roleId:guid}/assignments")]
    public async Task<IActionResult> RevokeRole(Guid roleId, RevokeRoleRequest request, CancellationToken cancellationToken)
    {
        var command = new RevokeRoleCommand(roleId, request.UserId, request.ScopeId);
        await _sender.Send(command, cancellationToken);
        return NoContent();
    }
}
