namespace AdminApi.API.Services;

public interface IDirectoryServiceClient
{
    Task<List<Models.OrganizationSummary>> GetOrganizationsAsync(CancellationToken cancellationToken = default);
    Task<int> GetOrganizationCountAsync(CancellationToken cancellationToken = default);
}
