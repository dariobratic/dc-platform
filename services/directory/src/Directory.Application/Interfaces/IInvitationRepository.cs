using Directory.Domain.Entities;

namespace Directory.Application.Interfaces;

public interface IInvitationRepository
{
    Task<Invitation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Invitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Invitation>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default);
    Task AddAsync(Invitation invitation, CancellationToken cancellationToken = default);
    Task UpdateAsync(Invitation invitation, CancellationToken cancellationToken = default);
}
