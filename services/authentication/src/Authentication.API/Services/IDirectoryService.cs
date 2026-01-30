using Authentication.API.Models;

namespace Authentication.API.Services;

public interface IDirectoryService
{
    Task<DirectoryOrganizationResponse> CreateOrganizationAsync(string name, string slug, string accessToken, CancellationToken cancellationToken = default);
    Task<DirectoryWorkspaceResponse> CreateWorkspaceAsync(string organizationId, string name, string slug, string accessToken, CancellationToken cancellationToken = default);
    Task<DirectoryMembershipResponse> AddMemberAsync(string workspaceId, string userId, string role, string accessToken, CancellationToken cancellationToken = default);
}
