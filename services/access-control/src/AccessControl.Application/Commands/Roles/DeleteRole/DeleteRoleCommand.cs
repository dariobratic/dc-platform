using MediatR;

namespace AccessControl.Application.Commands.Roles.DeleteRole;

public sealed record DeleteRoleCommand(Guid Id) : IRequest;
