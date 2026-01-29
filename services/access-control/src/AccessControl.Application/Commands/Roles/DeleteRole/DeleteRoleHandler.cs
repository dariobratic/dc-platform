using AccessControl.Application.Exceptions;
using AccessControl.Application.Interfaces;
using AccessControl.Domain.Exceptions;
using MediatR;

namespace AccessControl.Application.Commands.Roles.DeleteRole;

public sealed class DeleteRoleHandler : IRequestHandler<DeleteRoleCommand>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IRoleAssignmentRepository _roleAssignmentRepository;

    public DeleteRoleHandler(
        IRoleRepository roleRepository,
        IRoleAssignmentRepository roleAssignmentRepository)
    {
        _roleRepository = roleRepository;
        _roleAssignmentRepository = roleAssignmentRepository;
    }

    public async Task Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(request.Id, cancellationToken);
        if (role == null)
            throw new NotFoundException("Role", request.Id);

        var assignments = await _roleAssignmentRepository.GetByRoleIdAsync(request.Id, cancellationToken);
        if (assignments.Count > 0)
            throw new DomainException("Cannot delete role with active assignments.");

        role.Delete();

        await _roleRepository.DeleteAsync(role, cancellationToken);
    }
}
