using AccessControl.Application.Exceptions;
using AccessControl.Application.Interfaces;
using AccessControl.Application.Responses;
using AccessControl.Domain.Entities;
using AccessControl.Domain.Exceptions;
using MediatR;

namespace AccessControl.Application.Commands.RoleAssignments.AssignRole;

public sealed class AssignRoleHandler : IRequestHandler<AssignRoleCommand, RoleAssignmentResponse>
{
    private readonly IRoleRepository _roleRepository;
    private readonly IRoleAssignmentRepository _roleAssignmentRepository;

    public AssignRoleHandler(
        IRoleRepository roleRepository,
        IRoleAssignmentRepository roleAssignmentRepository)
    {
        _roleRepository = roleRepository;
        _roleAssignmentRepository = roleAssignmentRepository;
    }

    public async Task<RoleAssignmentResponse> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _roleRepository.GetByIdAsync(request.RoleId, cancellationToken);
        if (role == null)
            throw new NotFoundException("Role", request.RoleId);

        if (role.ScopeId != request.ScopeId || role.ScopeType != request.ScopeType)
            throw new DomainException("Role scope does not match the assignment scope.");

        var exists = await _roleAssignmentRepository.ExistsAsync(
            request.RoleId,
            request.UserId,
            request.ScopeId,
            cancellationToken);

        if (exists)
            throw new ConflictException("Role assignment already exists.");

        var assignment = RoleAssignment.Create(
            request.RoleId,
            request.UserId,
            request.ScopeId,
            request.ScopeType,
            request.AssignedBy);

        await _roleAssignmentRepository.AddAsync(assignment, cancellationToken);

        // Reload to get navigation properties
        var savedAssignment = await _roleAssignmentRepository.GetByIdAsync(assignment.Id, cancellationToken);

        return RoleAssignmentResponse.FromEntity(savedAssignment!);
    }
}
