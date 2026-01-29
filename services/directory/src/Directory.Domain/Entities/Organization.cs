using Directory.Domain.Exceptions;
using Directory.Domain.ValueObjects;

namespace Directory.Domain.Entities;

public class Organization
{
    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;
    public Slug Slug { get; private set; } = null!;
    public OrganizationStatus Status { get; private set; }
    public OrganizationSettings Settings { get; private set; } = null!;
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    private readonly List<Workspace> _workspaces = [];
    public IReadOnlyCollection<Workspace> Workspaces => _workspaces.AsReadOnly();

    private Organization() { }

    public static Organization Create(string name, Slug slug, OrganizationSettings? settings = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Organization name cannot be empty.");

        return new Organization
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            Slug = slug,
            Status = OrganizationStatus.Active,
            Settings = settings ?? OrganizationSettings.Empty(),
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, OrganizationSettings? settings = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Organization name cannot be empty.");

        Name = name.Trim();

        if (settings is not null)
            Settings = settings;

        UpdatedAt = DateTime.UtcNow;
    }

    public void Suspend()
    {
        if (Status == OrganizationStatus.Deleted)
            throw new DomainException("Cannot suspend a deleted organization.");

        Status = OrganizationStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (Status == OrganizationStatus.Deleted)
            throw new DomainException("Cannot activate a deleted organization.");

        Status = OrganizationStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        if (Status == OrganizationStatus.Deleted)
            throw new DomainException("Organization is already deleted.");

        Status = OrganizationStatus.Deleted;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
