namespace AdminApi.API.Models;

public record SystemHealthResponse(
    string OverallStatus,
    List<ServiceHealthStatus> Services,
    DateTime CheckedAt);

public record ServiceHealthStatus(
    string ServiceName,
    string Status,
    int? StatusCode,
    long? ResponseTimeMs);
