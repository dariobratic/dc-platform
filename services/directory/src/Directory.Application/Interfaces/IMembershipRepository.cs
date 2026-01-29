using Directory.Domain.Entities;

namespace Directory.Application.Interfaces;

public interface IMembershipRepository
{
    Task<Membership?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Membership>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Membership>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Membership?> GetByWorkspaceAndUserAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default);
}
