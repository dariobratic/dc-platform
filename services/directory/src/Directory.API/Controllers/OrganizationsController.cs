using Directory.API.DTOs;
using Directory.Application.Commands.Organizations;
using Directory.Application.Queries.Organizations;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Directory.API.Controllers;

[ApiController]
[Route("api/v1/organizations")]
public class OrganizationsController : ControllerBase
{
    private readonly ISender _sender;

    public OrganizationsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpPost]
    [ProducesResponseType(typeof(OrganizationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(CreateOrganizationRequest request, CancellationToken cancellationToken)
    {
        var command = new CreateOrganizationCommand(request.Name, request.Slug, request.Settings);
        var result = await _sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OrganizationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetOrganizationByIdQuery(id);
        var result = await _sender.Send(query, cancellationToken);
        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(OrganizationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, UpdateOrganizationRequest request, CancellationToken cancellationToken)
    {
        var command = new UpdateOrganizationCommand(id, request.Name, request.Settings);
        var result = await _sender.Send(command, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var command = new DeleteOrganizationCommand(id);
        await _sender.Send(command, cancellationToken);
        return NoContent();
    }
}
