using Directory.Application.Interfaces;
using Directory.Domain.Entities;
using Directory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Directory.Infrastructure.Repositories;

public class WorkspaceRepository : IWorkspaceRepository
{
    private readonly DirectoryDbContext _context;

    public WorkspaceRepository(DirectoryDbContext context)
    {
        _context = context;
    }

    public async Task<Workspace?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Workspaces
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<Workspace?> GetByIdWithMembershipsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Workspaces
            .Include(w => w.Memberships)
            .FirstOrDefaultAsync(w => w.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Workspace>> GetByOrganizationIdAsync(Guid organizationId, CancellationToken cancellationToken = default)
    {
        return await _context.Workspaces
            .Where(w => w.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> SlugExistsInOrganizationAsync(Guid organizationId, string slug, CancellationToken cancellationToken = default)
    {
        return await _context.Workspaces
            .AnyAsync(w => w.OrganizationId == organizationId && w.Slug == Domain.ValueObjects.Slug.FromExisting(slug), cancellationToken);
    }

    public async Task AddAsync(Workspace workspace, CancellationToken cancellationToken = default)
    {
        _context.Workspaces.Add(workspace);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Workspace workspace, CancellationToken cancellationToken = default)
    {
        await _context.SaveChangesAsync(cancellationToken);
    }
}
