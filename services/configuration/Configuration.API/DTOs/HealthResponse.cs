namespace Configuration.API.DTOs;

public record HealthResponse(
    string ServiceName,
    string Status,
    DateTime Timestamp
);
