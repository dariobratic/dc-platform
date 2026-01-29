using AccessControl.Domain.Entities;
using AccessControl.Domain.Enums;

namespace AccessControl.Application.Interfaces;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Role?> GetByIdWithPermissionsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<Role>> GetByScopeAsync(Guid scopeId, ScopeType scopeType, CancellationToken cancellationToken = default);
    Task<bool> NameExistsInScopeAsync(string name, Guid scopeId, ScopeType scopeType, Guid? excludeRoleId = null, CancellationToken cancellationToken = default);
    Task AddAsync(Role role, CancellationToken cancellationToken = default);
    Task UpdateAsync(Role role, CancellationToken cancellationToken = default);
    Task DeleteAsync(Role role, CancellationToken cancellationToken = default);
}
