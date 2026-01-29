using Directory.Domain.Entities;
using Directory.Domain.Events;
using Directory.Domain.Exceptions;
using FluentAssertions;

namespace Directory.Domain.Tests.Entities;

public class InvitationTests
{
    private static readonly Guid DefaultWorkspaceId = Guid.NewGuid();
    private static readonly Guid DefaultInviterId = Guid.NewGuid();
    private const string DefaultEmail = "user@example.com";

    public class CreateTests
    {
        [Fact]
        public void Create_WithValidData_SetsProperties()
        {
            var invitation = Invitation.Create(DefaultWorkspaceId, DefaultEmail, WorkspaceRole.Member, DefaultInviterId);

            invitation.Id.Should().NotBeEmpty();
            invitation.WorkspaceId.Should().Be(DefaultWorkspaceId);
            invitation.Email.Should().Be(DefaultEmail);
            invitation.Role.Should().Be(WorkspaceRole.Member);
            invitation.Token.Should().NotBeNullOrEmpty();
            invitation.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(1));
            invitation.Status.Should().Be(InvitationStatus.Pending);
            invitation.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            invitation.AcceptedAt.Should().BeNull();
            invitation.InvitedBy.Should().Be(DefaultInviterId);
        }

        [Fact]
        public void Create_WithCustomExpiry_UsesCustomExpiry()
        {
            var customExpiry = TimeSpan.FromHours(24);

            var invitation = Invitation.Create(DefaultWorkspaceId, DefaultEmail, WorkspaceRole.Admin, DefaultInviterId, customExpiry);

            invitation.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.Add(customExpiry), TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Create_WithoutExpiry_UsesDefaultSevenDays()
        {
            var invitation = Invitation.Create(DefaultWorkspaceId, DefaultEmail, WorkspaceRole.Member, DefaultInviterId);

            invitation.ExpiresAt.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Create_NormalizesEmail()
        {
            var invitation = Invitation.Create(DefaultWorkspaceId, "  USER@EXAMPLE.COM  ", WorkspaceRole.Member, DefaultInviterId);

            invitation.Email.Should().Be("user@example.com");
        }

        [Theory]
        [InlineData(WorkspaceRole.Owner)]
        [InlineData(WorkspaceRole.Admin)]
        [InlineData(WorkspaceRole.Member)]
        [InlineData(WorkspaceRole.Viewer)]
        public void Create_WithDifferentRoles_AssignsCorrectRole(WorkspaceRole role)
        {
            var invitation = Invitation.Create(DefaultWorkspaceId, DefaultEmail, role, DefaultInviterId);

            invitation.Role.Should().Be(role);
        }

        [Fact]
        public void Create_GeneratesUniqueTokens()
        {
            var invitation1 = Invitation.Create(DefaultWorkspaceId, DefaultEmail, WorkspaceRole.Member, DefaultInviterId);
            var invitation2 = Invitation.Create(DefaultWorkspaceId, DefaultEmail, WorkspaceRole.Member, DefaultInviterId);

            invitation1.Token.Should().NotBe(invitation2.Token);
        }

        [Fact]
        public void Create_GeneratesUrlSafeToken()
        {
            var invitation = Invitation.Create(DefaultWorkspaceId, DefaultEmail, WorkspaceRole.Member, DefaultInviterId);

            invitation.Token.Should().NotContain("+");
            invitation.Token.Should().NotContain("/");
            invitation.Token.Should().NotContain("=");
        }

        [Fact]
        public void Create_WithEmptyWorkspaceId_ThrowsDomainException()
        {
            var act = () => Invitation.Create(Guid.Empty, DefaultEmail, WorkspaceRole.Member, DefaultInviterId);

            act.Should().Throw<DomainException>()
                .WithMessage("*Workspace ID*required*");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithEmptyEmail_ThrowsDomainException(string? email)
        {
            var act = () => Invitation.Create(DefaultWorkspaceId, email!, WorkspaceRole.Member, DefaultInviterId);

            act.Should().Throw<DomainException>()
                .WithMessage("*Email*required*");
        }

        [Fact]
        public void Create_WithEmptyInvitedBy_ThrowsDomainException()
        {
            var act = () => Invitation.Create(DefaultWorkspaceId, DefaultEmail, WorkspaceRole.Member, Guid.Empty);

            act.Should().Throw<DomainException>()
                .WithMessage("*InvitedBy*required*");
        }

        [Fact]
        public void Create_RaisesInvitationCreatedEvent()
        {
            var invitation = Invitation.Create(DefaultWorkspaceId, DefaultEmail, WorkspaceRole.Admin, DefaultInviterId);

            invitation.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<InvitationCreated>()
                .Which.Should().BeEquivalentTo(new
                {
                    InvitationId = invitation.Id,
                    WorkspaceId = DefaultWorkspaceId,
                    Email = DefaultEmail,
                    Role = WorkspaceRole.Admin,
                    InvitedBy = DefaultInviterId
                });
        }
    }

    public class AcceptTests
    {
        [Fact]
        public void Accept_WhenPendingAndNotExpired_AcceptsInvitation()
        {
            var invitation = Invitation.Create(DefaultWorkspaceId, DefaultEmail, WorkspaceRole.Member, DefaultInviterId);
            invitation.ClearDomainEvents();

            invitation.Accept();

            invitation.Status.Should().Be(InvitationStatus.Accepted);
            invitation.AcceptedAt.Should().NotBeNull();
            invitation.AcceptedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        }

        [Fact]
        public void Accept_WhenAccepted_ThrowsDomainException()
        {
            var invitation = Invitation.Create(DefaultWorkspaceId, DefaultEmail, WorkspaceRole.Member, DefaultInviterId);
            invitation.Accept();

            var act = () => invitation.Accept();

            act.Should().Throw<DomainException>()
                .WithMessage("*Cannot accept*Accepted*");
        }

        [Fact]
        public void Accept_WhenRevoked_ThrowsDomainException()
        {
            var invitation = Invitation.Create(DefaultWorkspaceId, DefaultEmail, WorkspaceRole.Member, DefaultInviterId);
            invitation.Revoke();

            var act = () => invitation.Accept();

            act.Should().Throw<DomainException>()
                .WithMessage("*Cannot accept*Revoked*");
        }

        [Fact]
        public void Accept_WhenExpired_ThrowsDomainExceptionAndSetsStatusToExpired()
        {
            var invitation = Invitation.Create(DefaultWorkspaceId, DefaultEmail, WorkspaceRole.Member, DefaultInviterId, TimeSpan.FromMilliseconds(-100));

            var act = () => invitation.Accept();

            act.Should().Throw<DomainException>()
                .WithMessage("*expired*");
            invitation.Status.Should().Be(InvitationStatus.Expired);
        }

        [Fact]
        public void Accept_RaisesInvitationAcceptedEvent()
        {
            var invitation = Invitation.Create(DefaultWorkspaceId, DefaultEmail, WorkspaceRole.Member, DefaultInviterId);
            invitation.ClearDomainEvents();

            invitation.Accept();

            invitation.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<InvitationAccepted>()
                .Which.Should().BeEquivalentTo(new
                {
                    InvitationId = invitation.Id,
                    WorkspaceId = DefaultWorkspaceId,
                    Email = DefaultEmail
                });
        }
    }

    public class RevokeTests
    {
        [Fact]
        public void Revoke_WhenPending_RevokesInvitation()
        {
            var invitation = Invitation.Create(DefaultWorkspaceId, DefaultEmail, WorkspaceRole.Member, DefaultInviterId);

            invitation.Revoke();

            invitation.Status.Should().Be(InvitationStatus.Revoked);
        }

        [Fact]
        public void Revoke_WhenAccepted_ThrowsDomainException()
        {
            var invitation = Invitation.Create(DefaultWorkspaceId, DefaultEmail, WorkspaceRole.Member, DefaultInviterId);
            invitation.Accept();

            var act = () => invitation.Revoke();

            act.Should().Throw<DomainException>()
                .WithMessage("*Cannot revoke*Accepted*");
        }

        [Fact]
        public void Revoke_WhenExpired_ThrowsDomainException()
        {
            var invitation = Invitation.Create(DefaultWorkspaceId, DefaultEmail, WorkspaceRole.Member, DefaultInviterId, TimeSpan.FromMilliseconds(-100));
            try { invitation.Accept(); } catch { }

            var act = () => invitation.Revoke();

            act.Should().Throw<DomainException>()
                .WithMessage("*Cannot revoke*Expired*");
        }

        [Fact]
        public void Revoke_WhenAlreadyRevoked_ThrowsDomainException()
        {
            var invitation = Invitation.Create(DefaultWorkspaceId, DefaultEmail, WorkspaceRole.Member, DefaultInviterId);
            invitation.Revoke();

            var act = () => invitation.Revoke();

            act.Should().Throw<DomainException>()
                .WithMessage("*Cannot revoke*Revoked*");
        }

        [Fact]
        public void Revoke_DoesNotRaiseEvent()
        {
            var invitation = Invitation.Create(DefaultWorkspaceId, DefaultEmail, WorkspaceRole.Member, DefaultInviterId);
            invitation.ClearDomainEvents();

            invitation.Revoke();

            invitation.DomainEvents.Should().BeEmpty();
        }
    }

    public class IsExpiredTests
    {
        [Fact]
        public void IsExpired_WhenPendingAndPastExpiry_ReturnsTrue()
        {
            var invitation = Invitation.Create(DefaultWorkspaceId, DefaultEmail, WorkspaceRole.Member, DefaultInviterId, TimeSpan.FromMilliseconds(-100));

            invitation.IsExpired.Should().BeTrue();
        }

        [Fact]
        public void IsExpired_WhenPendingAndBeforeExpiry_ReturnsFalse()
        {
            var invitation = Invitation.Create(DefaultWorkspaceId, DefaultEmail, WorkspaceRole.Member, DefaultInviterId);

            invitation.IsExpired.Should().BeFalse();
        }

        [Fact]
        public void IsExpired_WhenAccepted_ReturnsFalse()
        {
            var invitation = Invitation.Create(DefaultWorkspaceId, DefaultEmail, WorkspaceRole.Member, DefaultInviterId);
            invitation.Accept();

            invitation.IsExpired.Should().BeFalse();
        }

        [Fact]
        public void IsExpired_WhenRevoked_ReturnsFalse()
        {
            var invitation = Invitation.Create(DefaultWorkspaceId, DefaultEmail, WorkspaceRole.Member, DefaultInviterId, TimeSpan.FromMilliseconds(-100));
            invitation.Revoke();

            invitation.IsExpired.Should().BeFalse();
        }

        [Fact]
        public void IsExpired_WhenExpiredStatus_ReturnsFalse()
        {
            var invitation = Invitation.Create(DefaultWorkspaceId, DefaultEmail, WorkspaceRole.Member, DefaultInviterId, TimeSpan.FromMilliseconds(-100));
            try { invitation.Accept(); } catch { }

            invitation.IsExpired.Should().BeFalse();
        }
    }

    public class DomainEventsTests
    {
        [Fact]
        public void ClearDomainEvents_RemovesAllEvents()
        {
            var invitation = Invitation.Create(DefaultWorkspaceId, DefaultEmail, WorkspaceRole.Member, DefaultInviterId);
            invitation.DomainEvents.Should().NotBeEmpty();

            invitation.ClearDomainEvents();

            invitation.DomainEvents.Should().BeEmpty();
        }

        [Fact]
        public void MultipleOperations_AccumulateEvents()
        {
            var invitation = Invitation.Create(DefaultWorkspaceId, DefaultEmail, WorkspaceRole.Member, DefaultInviterId);
            invitation.Accept();

            invitation.DomainEvents.Should().HaveCount(2);
            invitation.DomainEvents.Select(e => e.GetType()).Should().ContainInOrder(
                typeof(InvitationCreated),
                typeof(InvitationAccepted));
        }
    }
}
