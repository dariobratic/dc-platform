using Directory.Domain.Entities;

namespace Directory.API.DTOs;

public sealed record AddMemberRequest(
    Guid UserId,
    WorkspaceRole Role);
