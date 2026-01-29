using Directory.Domain.Exceptions;

namespace Directory.Domain.Entities;

public class Invitation
{
    public Guid Id { get; private set; }
    public Guid WorkspaceId { get; private set; }
    public string Email { get; private set; } = null!;
    public WorkspaceRole Role { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime ExpiresAt { get; private set; }
    public InvitationStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? AcceptedAt { get; private set; }
    public Guid InvitedBy { get; private set; }

    public Workspace Workspace { get; private set; } = null!;

    private Invitation() { }

    public static Invitation Create(Guid workspaceId, string email, WorkspaceRole role, Guid invitedBy, TimeSpan? expiry = null)
    {
        if (workspaceId == Guid.Empty)
            throw new DomainException("Workspace ID is required.");

        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required.");

        if (invitedBy == Guid.Empty)
            throw new DomainException("InvitedBy user ID is required.");

        var expiresIn = expiry ?? TimeSpan.FromDays(7);

        return new Invitation
        {
            Id = Guid.NewGuid(),
            WorkspaceId = workspaceId,
            Email = email.Trim().ToLowerInvariant(),
            Role = role,
            Token = GenerateToken(),
            ExpiresAt = DateTime.UtcNow.Add(expiresIn),
            Status = InvitationStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            InvitedBy = invitedBy
        };
    }

    public void Accept()
    {
        if (Status != InvitationStatus.Pending)
            throw new DomainException($"Cannot accept an invitation with status '{Status}'.");

        if (DateTime.UtcNow > ExpiresAt)
        {
            Status = InvitationStatus.Expired;
            throw new DomainException("Invitation has expired.");
        }

        Status = InvitationStatus.Accepted;
        AcceptedAt = DateTime.UtcNow;
    }

    public void Revoke()
    {
        if (Status != InvitationStatus.Pending)
            throw new DomainException($"Cannot revoke an invitation with status '{Status}'.");

        Status = InvitationStatus.Revoked;
    }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt && Status == InvitationStatus.Pending;

    private static string GenerateToken()
    {
        return Convert.ToBase64String(Guid.NewGuid().ToByteArray())
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }
}
