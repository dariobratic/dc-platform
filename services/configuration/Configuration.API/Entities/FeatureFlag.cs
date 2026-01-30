namespace Configuration.API.Entities;

public class FeatureFlag
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Key { get; set; } = null!;
    public string Description { get; set; } = null!;
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
