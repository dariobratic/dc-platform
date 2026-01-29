using Audit.API.DTOs;
using Audit.Application.Commands.CreateAuditEntry;
using Audit.Application.Queries.GetAuditEntries;
using Audit.Application.Queries.GetAuditEntryById;
using Audit.Application.Queries.GetEntityAuditHistory;
using Audit.Application.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Audit.API.Controllers;

[ApiController]
[Route("api/v1/audit")]
public class AuditController : ControllerBase
{
    private readonly ISender _sender;

    public AuditController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(typeof(AuditEntryResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateAuditEntryRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateAuditEntryCommand(
            request.UserId,
            request.Action,
            request.EntityType,
            request.EntityId,
            request.ServiceName,
            request.UserEmail,
            request.OrganizationId,
            request.WorkspaceId,
            request.Details,
            request.IpAddress,
            request.UserAgent,
            request.CorrelationId);

        var result = await _sender.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(AuditEntryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetAuditEntryByIdQuery(id);
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PagedResponse<AuditEntryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? organizationId,
        [FromQuery] Guid? workspaceId,
        [FromQuery] Guid? userId,
        [FromQuery] string? entityType,
        [FromQuery] string? action,
        [FromQuery] string? serviceName,
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAuditEntriesQuery(
            organizationId,
            workspaceId,
            userId,
            entityType,
            action,
            serviceName,
            from,
            to,
            skip,
            take);

        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpGet("entity/{entityType}/{entityId:guid}")]
    [ProducesResponseType(typeof(List<AuditEntryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEntityHistory(
        string entityType,
        Guid entityId,
        CancellationToken cancellationToken)
    {
        var query = new GetEntityAuditHistoryQuery(entityType, entityId);
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }
}
