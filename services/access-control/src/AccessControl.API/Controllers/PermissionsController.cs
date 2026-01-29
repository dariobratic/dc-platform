using AccessControl.Application.Queries.Permissions.CheckPermission;
using AccessControl.Application.Queries.Permissions.GetUserPermissions;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AccessControl.API.Controllers;

[ApiController]
[Route("api/v1/permissions")]
public class PermissionsController : ControllerBase
{
    private readonly ISender _sender;

    public PermissionsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet("check")]
    public async Task<IActionResult> CheckPermission(
        [FromQuery] Guid userId,
        [FromQuery] Guid scopeId,
        [FromQuery] string permission,
        CancellationToken cancellationToken)
    {
        var query = new CheckPermissionQuery(userId, scopeId, permission);
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("/api/v1/users/{userId:guid}/permissions")]
    public async Task<IActionResult> GetUserPermissions(
        Guid userId,
        [FromQuery] Guid scopeId,
        CancellationToken cancellationToken)
    {
        var query = new GetUserPermissionsQuery(userId, scopeId);
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }
}
