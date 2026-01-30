namespace AdminApi.API.Services;

public interface IAuditServiceClient
{
    Task<List<Models.AuditEntrySummary>> GetRecentEntriesAsync(int count = 20, CancellationToken cancellationToken = default);
    Task<int> GetTotalCountAsync(CancellationToken cancellationToken = default);
}
