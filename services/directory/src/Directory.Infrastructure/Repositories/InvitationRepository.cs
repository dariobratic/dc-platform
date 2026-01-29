using Directory.Application.Interfaces;
using Directory.Domain.Entities;
using Directory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Directory.Infrastructure.Repositories;

public class InvitationRepository : IInvitationRepository
{
    private readonly DirectoryDbContext _context;

    public InvitationRepository(DirectoryDbContext context)
    {
        _context = context;
    }

    public async Task<Invitation?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Invitations
            .FirstOrDefaultAsync(i => i.Id == id, cancellationToken);
    }

    public async Task<Invitation?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.Invitations
            .Include(i => i.Workspace)
            .ThenInclude(w => w.Organization)
            .FirstOrDefaultAsync(i => i.Token == token, cancellationToken);
    }

    public async Task<IReadOnlyList<Invitation>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        return await _context.Invitations
            .Where(i => i.WorkspaceId == workspaceId)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Invitation invitation, CancellationToken cancellationToken = default)
    {
        _context.Invitations.Add(invitation);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Invitation invitation, CancellationToken cancellationToken = default)
    {
        _context.Invitations.Update(invitation);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
