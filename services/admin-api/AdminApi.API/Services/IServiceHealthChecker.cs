namespace AdminApi.API.Services;

public interface IServiceHealthChecker
{
    Task<Models.SystemHealthResponse> CheckAllServicesAsync(CancellationToken cancellationToken = default);
}
