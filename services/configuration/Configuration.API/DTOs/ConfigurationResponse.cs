namespace Configuration.API.DTOs;

public record ConfigurationResponse(
    Guid OrganizationId,
    Dictionary<string, string> Settings,
    DateTime? LastUpdated
);
