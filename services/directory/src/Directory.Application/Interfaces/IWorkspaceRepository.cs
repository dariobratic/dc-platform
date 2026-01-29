using Directory.Domain.Entities;

namespace Directory.Application.Interfaces;

public interface IWorkspaceRepository
{
    Task<Workspace?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Workspace?> GetByIdWithMembershipsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Workspace>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default);
    Task<bool> SlugExistsInOrganizationAsync(Guid organizationId, string slug, CancellationToken cancellationToken = default);
    Task AddAsync(Workspace workspace, CancellationToken cancellationToken = default);
    Task UpdateAsync(Workspace workspace, CancellationToken cancellationToken = default);
}
