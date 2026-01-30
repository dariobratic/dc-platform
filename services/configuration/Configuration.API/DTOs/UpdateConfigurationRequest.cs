namespace Configuration.API.DTOs;

public record UpdateConfigurationRequest(
    Dictionary<string, string> Settings
);
