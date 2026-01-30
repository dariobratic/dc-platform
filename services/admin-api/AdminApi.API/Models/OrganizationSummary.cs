namespace AdminApi.API.Models;

public record OrganizationSummary(
    Guid Id,
    string Name,
    string Slug,
    string Status,
    DateTime CreatedAt);
