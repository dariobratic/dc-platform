namespace Directory.API.DTOs;

public sealed record UpdateOrganizationRequest(
    string Name,
    Dictionary<string, string>? Settings);
