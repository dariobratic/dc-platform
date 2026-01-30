namespace AdminApi.API.Models;

public record HealthResponse(
    string ServiceName,
    string Status,
    DateTime Timestamp);
