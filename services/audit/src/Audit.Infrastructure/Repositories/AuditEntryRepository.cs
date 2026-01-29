using Audit.Application.Interfaces;
using Audit.Domain.Entities;
using Audit.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Audit.Infrastructure.Repositories;

public class AuditEntryRepository : IAuditEntryRepository
{
    private readonly AuditDbContext _context;

    public AuditEntryRepository(AuditDbContext context)
    {
        _context = context;
    }

    public async Task<AuditEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.AuditEntries
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public async Task<(List<AuditEntry> Items, int TotalCount)> GetPagedAsync(
        Guid? organizationId = null,
        Guid? workspaceId = null,
        Guid? userId = null,
        string? entityType = null,
        string? action = null,
        string? serviceName = null,
        DateTime? from = null,
        DateTime? to = null,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        var query = _context.AuditEntries.AsQueryable();

        if (organizationId.HasValue)
            query = query.Where(e => e.OrganizationId == organizationId.Value);
        if (workspaceId.HasValue)
            query = query.Where(e => e.WorkspaceId == workspaceId.Value);
        if (userId.HasValue)
            query = query.Where(e => e.UserId == userId.Value);
        if (!string.IsNullOrEmpty(entityType))
            query = query.Where(e => e.EntityType == entityType);
        if (!string.IsNullOrEmpty(action))
            query = query.Where(e => e.Action == action);
        if (!string.IsNullOrEmpty(serviceName))
            query = query.Where(e => e.ServiceName == serviceName);
        if (from.HasValue)
            query = query.Where(e => e.Timestamp >= from.Value);
        if (to.HasValue)
            query = query.Where(e => e.Timestamp <= to.Value);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .OrderByDescending(e => e.Timestamp)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<List<AuditEntry>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default)
    {
        return await _context.AuditEntries
            .Where(e => e.EntityType == entityType && e.EntityId == entityId)
            .OrderByDescending(e => e.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(AuditEntry entry, CancellationToken cancellationToken = default)
    {
        await _context.AuditEntries.AddAsync(entry, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
