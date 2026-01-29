using Directory.Domain.Entities;

namespace Directory.API.DTOs;

public sealed record CreateInvitationRequest(
    string Email,
    WorkspaceRole Role);
