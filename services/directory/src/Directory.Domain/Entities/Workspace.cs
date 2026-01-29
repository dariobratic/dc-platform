using Directory.Domain.Events;
using Directory.Domain.Exceptions;
using Directory.Domain.ValueObjects;

namespace Directory.Domain.Entities;

public class Workspace : BaseEntity
{
    public Guid Id { get; private set; }
    public Guid OrganizationId { get; private set; }
    public string Name { get; private set; } = null!;
    public Slug Slug { get; private set; } = null!;
    public WorkspaceStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    public Organization Organization { get; private set; } = null!;

    private readonly List<Membership> _memberships = [];
    public IReadOnlyCollection<Membership> Memberships => _memberships.AsReadOnly();

    private readonly List<Invitation> _invitations = [];
    public IReadOnlyCollection<Invitation> Invitations => _invitations.AsReadOnly();

    private Workspace() { }

    public static Workspace Create(Guid organizationId, string name, Slug slug)
    {
        if (organizationId == Guid.Empty)
            throw new DomainException("Organization ID is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Workspace name cannot be empty.");

        var workspace = new Workspace
        {
            Id = Guid.NewGuid(),
            OrganizationId = organizationId,
            Name = name.Trim(),
            Slug = slug,
            Status = WorkspaceStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        workspace.RaiseDomainEvent(new WorkspaceCreated(workspace.Id, organizationId, workspace.Name, slug.Value));

        return workspace;
    }

    public void Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Workspace name cannot be empty.");

        Name = name.Trim();
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new WorkspaceUpdated(Id, Name));
    }

    public void Suspend()
    {
        if (Status == WorkspaceStatus.Deleted)
            throw new DomainException("Cannot suspend a deleted workspace.");

        Status = WorkspaceStatus.Suspended;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        if (Status == WorkspaceStatus.Deleted)
            throw new DomainException("Cannot activate a deleted workspace.");

        Status = WorkspaceStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete()
    {
        if (Status == WorkspaceStatus.Deleted)
            throw new DomainException("Workspace is already deleted.");

        Status = WorkspaceStatus.Deleted;
        DeletedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;

        RaiseDomainEvent(new WorkspaceDeleted(Id, OrganizationId));
    }

    public Membership AddMember(Guid userId, WorkspaceRole role, Guid? invitedBy = null)
    {
        if (Status != WorkspaceStatus.Active)
            throw new DomainException("Cannot add members to an inactive workspace.");

        if (_memberships.Any(m => m.UserId == userId))
            throw new DomainException("User is already a member of this workspace.");

        var membership = Membership.Create(Id, userId, role, invitedBy);
        _memberships.Add(membership);

        RaiseDomainEvent(new MemberAdded(Id, userId, role));

        return membership;
    }

    public void RemoveMember(Guid userId)
    {
        var membership = _memberships.FirstOrDefault(m => m.UserId == userId)
            ?? throw new DomainException("User is not a member of this workspace.");

        _memberships.Remove(membership);

        RaiseDomainEvent(new MemberRemoved(Id, userId));
    }

    public void ChangeMemberRole(Guid userId, WorkspaceRole newRole)
    {
        var membership = _memberships.FirstOrDefault(m => m.UserId == userId)
            ?? throw new DomainException("User is not a member of this workspace.");

        var oldRole = membership.Role;
        if (oldRole == newRole)
            return;

        membership.ChangeRole(newRole);

        RaiseDomainEvent(new MemberRoleChanged(Id, userId, oldRole, newRole));
    }
}
