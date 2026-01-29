using Audit.Domain.Entities;

namespace Audit.Application.Interfaces;

public interface IAuditEntryRepository
{
    Task<AuditEntry?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(List<AuditEntry> Items, int TotalCount)> GetPagedAsync(
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
        CancellationToken cancellationToken = default);
    Task<List<AuditEntry>> GetByEntityAsync(string entityType, Guid entityId, CancellationToken cancellationToken = default);
    Task AddAsync(AuditEntry entry, CancellationToken cancellationToken = default);
}
