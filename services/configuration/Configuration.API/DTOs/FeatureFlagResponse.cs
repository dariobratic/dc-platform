namespace Configuration.API.DTOs;

public record FeatureFlagResponse(
    string Key,
    string Description,
    bool IsEnabled,
    DateTime? UpdatedAt
);
