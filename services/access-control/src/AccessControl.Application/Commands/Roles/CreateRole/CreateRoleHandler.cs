using AccessControl.Application.Exceptions;
using AccessControl.Application.Interfaces;
using AccessControl.Application.Responses;
using AccessControl.Domain.Entities;
using MediatR;

namespace AccessControl.Application.Commands.Roles.CreateRole;

public sealed class CreateRoleHandler : IRequestHandler<CreateRoleCommand, RoleResponse>
{
    private readonly IRoleRepository _roleRepository;

    public CreateRoleHandler(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<RoleResponse> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        var nameExists = await _roleRepository.NameExistsInScopeAsync(
            request.Name,
            request.ScopeId,
            request.ScopeType,
            null,
            cancellationToken);

        if (nameExists)
            throw new ConflictException($"Role with name '{request.Name}' already exists in this scope.");

        var role = Role.Create(request.Name, request.Description, request.ScopeId, request.ScopeType);

        foreach (var permission in request.Permissions)
        {
            role.AddPermission(permission);
        }

        await _roleRepository.AddAsync(role, cancellationToken);

        return RoleResponse.FromEntity(role);
    }
}
