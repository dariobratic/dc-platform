using AccessControl.Application.Responses;
using MediatR;

namespace AccessControl.Application.Queries.Roles.GetRoleById;

public sealed record GetRoleByIdQuery(Guid Id) : IRequest<RoleResponse>;
