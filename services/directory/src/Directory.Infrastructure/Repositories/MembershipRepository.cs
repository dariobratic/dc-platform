using Directory.Application.Interfaces;
using Directory.Domain.Entities;
using Directory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Directory.Infrastructure.Repositories;

public class MembershipRepository : IMembershipRepository
{
    private readonly DirectoryDbContext _context;

    public MembershipRepository(DirectoryDbContext context)
    {
        _context = context;
    }

    public async Task<Membership?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Memberships
            .FirstOrDefaultAsync(m => m.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Membership>> GetByWorkspaceIdAsync(Guid workspaceId, CancellationToken cancellationToken = default)
    {
        return await _context.Memberships
            .Where(m => m.WorkspaceId == workspaceId)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Membership>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Memberships
            .Include(m => m.Workspace)
            .Where(m => m.UserId == userId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Membership?> GetByWorkspaceAndUserAsync(Guid workspaceId, Guid userId, CancellationToken cancellationToken = default)
    {
        return await _context.Memberships
            .FirstOrDefaultAsync(m => m.WorkspaceId == workspaceId && m.UserId == userId, cancellationToken);
    }
}
