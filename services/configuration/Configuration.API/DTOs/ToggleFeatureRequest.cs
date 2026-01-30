namespace Configuration.API.DTOs;

public record ToggleFeatureRequest(
    bool IsEnabled,
    string? Description = null
);
