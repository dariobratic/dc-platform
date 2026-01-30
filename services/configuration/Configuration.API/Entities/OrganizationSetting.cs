namespace Configuration.API.Entities;

public class OrganizationSetting
{
    public Guid Id { get; set; }
    public Guid OrganizationId { get; set; }
    public string Key { get; set; } = null!;
    public string Value { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
