using AccessControl.Application.Interfaces;
using AccessControl.Domain.Entities;
using AccessControl.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccessControl.Infrastructure.Repositories;

public class RoleAssignmentRepository : IRoleAssignmentRepository
{
    private readonly AccessControlDbContext _context;

    public RoleAssignmentRepository(AccessControlDbContext context)
    {
        _context = context;
    }

    public async Task<RoleAssignment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.RoleAssignments
            .Include(ra => ra.Role)
            .FirstOrDefaultAsync(ra => ra.Id == id, cancellationToken);
    }

    public async Task<RoleAssignment?> FindAsync(Guid roleId, Guid userId, Guid scopeId, CancellationToken cancellationToken = default)
    {
        return await _context.RoleAssignments
            .Include(ra => ra.Role)
            .FirstOrDefaultAsync(ra =>
                ra.RoleId == roleId &&
                ra.UserId == userId &&
                ra.ScopeId == scopeId,
                cancellationToken);
    }

    public async Task<List<RoleAssignment>> GetByUserAndScopeAsync(Guid userId, Guid scopeId, CancellationToken cancellationToken = default)
    {
        return await _context.RoleAssignments
            .Include(ra => ra.Role)
            .ThenInclude(r => r.Permissions)
            .Where(ra => ra.UserId == userId && ra.ScopeId == scopeId)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<RoleAssignment>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        return await _context.RoleAssignments
            .Where(ra => ra.RoleId == roleId)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsAsync(Guid roleId, Guid userId, Guid scopeId, CancellationToken cancellationToken = default)
    {
        return await _context.RoleAssignments
            .AnyAsync(ra =>
                ra.RoleId == roleId &&
                ra.UserId == userId &&
                ra.ScopeId == scopeId,
                cancellationToken);
    }

    public async Task AddAsync(RoleAssignment assignment, CancellationToken cancellationToken = default)
    {
        await _context.RoleAssignments.AddAsync(assignment, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(RoleAssignment assignment, CancellationToken cancellationToken = default)
    {
        _context.RoleAssignments.Remove(assignment);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
