using Directory.Domain.Entities;
using Directory.Domain.Exceptions;
using FluentAssertions;

namespace Directory.Domain.Tests.Entities;

public class MembershipTests
{
    private static readonly Guid DefaultWorkspaceId = Guid.NewGuid();
    private static readonly Guid DefaultUserId = Guid.NewGuid();

    public class CreateTests
    {
        [Fact]
        public void Create_WithValidData_SetsProperties()
        {
            var membership = Membership.Create(DefaultWorkspaceId, DefaultUserId, WorkspaceRole.Member);

            membership.Id.Should().NotBeEmpty();
            membership.WorkspaceId.Should().Be(DefaultWorkspaceId);
            membership.UserId.Should().Be(DefaultUserId);
            membership.Role.Should().Be(WorkspaceRole.Member);
            membership.JoinedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            membership.InvitedBy.Should().BeNull();
        }

        [Fact]
        public void Create_WithInvitedBy_StoresInviter()
        {
            var inviterId = Guid.NewGuid();

            var membership = Membership.Create(DefaultWorkspaceId, DefaultUserId, WorkspaceRole.Admin, inviterId);

            membership.InvitedBy.Should().Be(inviterId);
        }

        [Theory]
        [InlineData(WorkspaceRole.Owner)]
        [InlineData(WorkspaceRole.Admin)]
        [InlineData(WorkspaceRole.Member)]
        [InlineData(WorkspaceRole.Viewer)]
        public void Create_WithDifferentRoles_AssignsCorrectRole(WorkspaceRole role)
        {
            var membership = Membership.Create(DefaultWorkspaceId, DefaultUserId, role);

            membership.Role.Should().Be(role);
        }

        [Fact]
        public void Create_WithEmptyWorkspaceId_ThrowsDomainException()
        {
            var act = () => Membership.Create(Guid.Empty, DefaultUserId, WorkspaceRole.Member);

            act.Should().Throw<DomainException>()
                .WithMessage("*Workspace ID*required*");
        }

        [Fact]
        public void Create_WithEmptyUserId_ThrowsDomainException()
        {
            var act = () => Membership.Create(DefaultWorkspaceId, Guid.Empty, WorkspaceRole.Member);

            act.Should().Throw<DomainException>()
                .WithMessage("*User ID*required*");
        }

        [Fact]
        public void Create_WithBothEmptyIds_ThrowsDomainException()
        {
            var act = () => Membership.Create(Guid.Empty, Guid.Empty, WorkspaceRole.Member);

            act.Should().Throw<DomainException>();
        }
    }

    public class ChangeRoleTests
    {
        [Fact]
        public void ChangeRole_UpdatesRole()
        {
            var membership = Membership.Create(DefaultWorkspaceId, DefaultUserId, WorkspaceRole.Member);

            membership.ChangeRole(WorkspaceRole.Admin);

            membership.Role.Should().Be(WorkspaceRole.Admin);
        }

        [Theory]
        [InlineData(WorkspaceRole.Member, WorkspaceRole.Owner)]
        [InlineData(WorkspaceRole.Viewer, WorkspaceRole.Admin)]
        [InlineData(WorkspaceRole.Admin, WorkspaceRole.Member)]
        [InlineData(WorkspaceRole.Owner, WorkspaceRole.Viewer)]
        public void ChangeRole_FromAnyRoleToAnyRole_Updates(WorkspaceRole initialRole, WorkspaceRole newRole)
        {
            var membership = Membership.Create(DefaultWorkspaceId, DefaultUserId, initialRole);

            membership.ChangeRole(newRole);

            membership.Role.Should().Be(newRole);
        }

        [Fact]
        public void ChangeRole_ToSameRole_AllowsChange()
        {
            var membership = Membership.Create(DefaultWorkspaceId, DefaultUserId, WorkspaceRole.Member);

            var act = () => membership.ChangeRole(WorkspaceRole.Member);

            act.Should().NotThrow();
            membership.Role.Should().Be(WorkspaceRole.Member);
        }
    }
}
