using Directory.Domain.Entities;

namespace Directory.Application.Queries.Workspaces;

public sealed record WorkspaceResponse(
    Guid Id,
    Guid OrganizationId,
    string Name,
    string Slug,
    WorkspaceStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public static WorkspaceResponse FromEntity(Workspace workspace)
    {
        return new WorkspaceResponse(
            workspace.Id,
            workspace.OrganizationId,
            workspace.Name,
            workspace.Slug.Value,
            workspace.Status,
            workspace.CreatedAt,
            workspace.UpdatedAt);
    }
}
