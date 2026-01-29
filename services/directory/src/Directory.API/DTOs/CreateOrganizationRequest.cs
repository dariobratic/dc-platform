namespace Directory.API.DTOs;

public sealed record CreateOrganizationRequest(
    string Name,
    string Slug,
    Dictionary<string, string>? Settings);
