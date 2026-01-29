using Directory.Domain.Entities;
using Directory.Domain.ValueObjects;

namespace Directory.Application.Queries.Organizations;

public sealed record OrganizationResponse(
    Guid Id,
    string Name,
    string Slug,
    OrganizationStatus Status,
    IReadOnlyDictionary<string, string> Settings,
    DateTime CreatedAt,
    DateTime? UpdatedAt)
{
    public static OrganizationResponse FromEntity(Organization organization)
    {
        return new OrganizationResponse(
            organization.Id,
            organization.Name,
            organization.Slug.Value,
            organization.Status,
            organization.Settings.Values,
            organization.CreatedAt,
            organization.UpdatedAt);
    }
}
