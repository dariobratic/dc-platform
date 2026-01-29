using Directory.Domain.Entities;

namespace Directory.API.DTOs;

public sealed record ChangeMemberRoleRequest(
    WorkspaceRole Role);
