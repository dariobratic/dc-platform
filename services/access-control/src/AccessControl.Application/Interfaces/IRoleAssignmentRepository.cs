using AccessControl.Domain.Entities;

namespace AccessControl.Application.Interfaces;

public interface IRoleAssignmentRepository
{
    Task<RoleAssignment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<RoleAssignment?> FindAsync(Guid roleId, Guid userId, Guid scopeId, CancellationToken cancellationToken = default);
    Task<List<RoleAssignment>> GetByUserAndScopeAsync(Guid userId, Guid scopeId, CancellationToken cancellationToken = default);
    Task<List<RoleAssignment>> GetByRoleIdAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid roleId, Guid userId, Guid scopeId, CancellationToken cancellationToken = default);
    Task AddAsync(RoleAssignment assignment, CancellationToken cancellationToken = default);
    Task DeleteAsync(RoleAssignment assignment, CancellationToken cancellationToken = default);
}
