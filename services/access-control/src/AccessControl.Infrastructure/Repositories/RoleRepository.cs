using AccessControl.Application.Interfaces;
using AccessControl.Domain.Entities;
using AccessControl.Domain.Enums;
using AccessControl.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AccessControl.Infrastructure.Repositories;

public class RoleRepository : IRoleRepository
{
    private readonly AccessControlDbContext _context;

    public RoleRepository(AccessControlDbContext context)
    {
        _context = context;
    }

    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Roles.FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<Role?> GetByIdWithPermissionsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .Include(r => r.Permissions)
            .FirstOrDefaultAsync(r => r.Id == id, cancellationToken);
    }

    public async Task<List<Role>> GetByScopeAsync(Guid scopeId, ScopeType scopeType, CancellationToken cancellationToken = default)
    {
        return await _context.Roles
            .Include(r => r.Permissions)
            .Where(r => r.ScopeId == scopeId && r.ScopeType == scopeType)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> NameExistsInScopeAsync(
        string name,
        Guid scopeId,
        ScopeType scopeType,
        Guid? excludeRoleId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Roles.Where(r =>
            r.Name == name &&
            r.ScopeId == scopeId &&
            r.ScopeType == scopeType);

        if (excludeRoleId.HasValue)
            query = query.Where(r => r.Id != excludeRoleId.Value);

        return await query.AnyAsync(cancellationToken);
    }

    public async Task AddAsync(Role role, CancellationToken cancellationToken = default)
    {
        await _context.Roles.AddAsync(role, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Role role, CancellationToken cancellationToken = default)
    {
        _context.Roles.Update(role);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Role role, CancellationToken cancellationToken = default)
    {
        _context.Roles.Remove(role);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
