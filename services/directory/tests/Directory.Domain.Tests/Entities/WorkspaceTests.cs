using Directory.Domain.Entities;
using Directory.Domain.Events;
using Directory.Domain.Exceptions;
using Directory.Domain.ValueObjects;
using FluentAssertions;

namespace Directory.Domain.Tests.Entities;

public class WorkspaceTests
{
    private static readonly Guid DefaultOrganizationId = Guid.NewGuid();
    private static readonly Slug DefaultSlug = Slug.Create("test-workspace");

    public class CreateTests
    {
        [Fact]
        public void Create_WithValidData_SetsProperties()
        {
            var workspace = Workspace.Create(DefaultOrganizationId, "Test Workspace", DefaultSlug);

            workspace.Id.Should().NotBeEmpty();
            workspace.OrganizationId.Should().Be(DefaultOrganizationId);
            workspace.Name.Should().Be("Test Workspace");
            workspace.Slug.Should().Be(DefaultSlug);
            workspace.Status.Should().Be(WorkspaceStatus.Active);
            workspace.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            workspace.UpdatedAt.Should().BeNull();
            workspace.DeletedAt.Should().BeNull();
            workspace.Memberships.Should().BeEmpty();
            workspace.Invitations.Should().BeEmpty();
        }

        [Fact]
        public void Create_TrimsName()
        {
            var workspace = Workspace.Create(DefaultOrganizationId, "  Test Workspace  ", DefaultSlug);

            workspace.Name.Should().Be("Test Workspace");
        }

        [Fact]
        public void Create_WithEmptyOrganizationId_ThrowsDomainException()
        {
            var act = () => Workspace.Create(Guid.Empty, "Test", DefaultSlug);

            act.Should().Throw<DomainException>()
                .WithMessage("*Organization ID*required*");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithEmptyName_ThrowsDomainException(string? name)
        {
            var act = () => Workspace.Create(DefaultOrganizationId, name!, DefaultSlug);

            act.Should().Throw<DomainException>()
                .WithMessage("*name*empty*");
        }

        [Fact]
        public void Create_RaisesWorkspaceCreatedEvent()
        {
            var workspace = Workspace.Create(DefaultOrganizationId, "Test Workspace", DefaultSlug);

            workspace.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<WorkspaceCreated>()
                .Which.Should().BeEquivalentTo(new
                {
                    WorkspaceId = workspace.Id,
                    OrganizationId = DefaultOrganizationId,
                    Name = "Test Workspace",
                    Slug = "test-workspace"
                });
        }
    }

    public class UpdateTests
    {
        [Fact]
        public void Update_WithValidName_UpdatesNameAndTimestamp()
        {
            var workspace = Workspace.Create(DefaultOrganizationId, "Original", DefaultSlug);
            workspace.ClearDomainEvents();

            workspace.Update("Updated Name");

            workspace.Name.Should().Be("Updated Name");
            workspace.UpdatedAt.Should().NotBeNull();
        }

        [Fact]
        public void Update_TrimsName()
        {
            var workspace = Workspace.Create(DefaultOrganizationId, "Original", DefaultSlug);

            workspace.Update("  Updated  ");

            workspace.Name.Should().Be("Updated");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Update_WithEmptyName_ThrowsDomainException(string? name)
        {
            var workspace = Workspace.Create(DefaultOrganizationId, "Test", DefaultSlug);

            var act = () => workspace.Update(name!);

            act.Should().Throw<DomainException>()
                .WithMessage("*name*empty*");
        }

        [Fact]
        public void Update_RaisesWorkspaceUpdatedEvent()
        {
            var workspace = Workspace.Create(DefaultOrganizationId, "Test", DefaultSlug);
            workspace.ClearDomainEvents();

            workspace.Update("Updated");

            workspace.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<WorkspaceUpdated>()
                .Which.Should().BeEquivalentTo(new
                {
                    WorkspaceId = workspace.Id,
                    Name = "Updated"
                });
        }
    }

    public class SuspendTests
    {
        [Fact]
        public void Suspend_FromActive_SetsSuspendedStatus()
        {
            var workspace = Workspace.Create(DefaultOrganizationId, "Test", DefaultSlug);

            workspace.Suspend();

            workspace.Status.Should().Be(WorkspaceStatus.Suspended);
            workspace.UpdatedAt.Should().NotBeNull();
        }

        [Fact]
        public void Suspend_WhenDeleted_ThrowsDomainException()
        {
            var workspace = Workspace.Create(DefaultOrganizationId, "Test", DefaultSlug);
            workspace.Delete();

            var act = () => workspace.Suspend();

            act.Should().Throw<DomainException>()
                .WithMessage("*suspend*deleted*");
        }
    }

    public class ActivateTests
    {
        [Fact]
        public void Activate_FromSuspended_SetsActiveStatus()
        {
            var workspace = Workspace.Create(DefaultOrganizationId, "Test", DefaultSlug);
            workspace.Suspend();

            workspace.Activate();

            workspace.Status.Should().Be(WorkspaceStatus.Active);
        }

        [Fact]
        public void Activate_WhenDeleted_ThrowsDomainException()
        {
            var workspace = Workspace.Create(DefaultOrganizationId, "Test", DefaultSlug);
            workspace.Delete();

            var act = () => workspace.Activate();

            act.Should().Throw<DomainException>()
                .WithMessage("*activate*deleted*");
        }
    }

    public class DeleteTests
    {
        [Fact]
        public void Delete_FromActive_SetsDeletedStatusAndTimestamp()
        {
            var workspace = Workspace.Create(DefaultOrganizationId, "Test", DefaultSlug);

            workspace.Delete();

            workspace.Status.Should().Be(WorkspaceStatus.Deleted);
            workspace.DeletedAt.Should().NotBeNull();
            workspace.UpdatedAt.Should().NotBeNull();
        }

        [Fact]
        public void Delete_FromSuspended_SetsDeletedStatus()
        {
            var workspace = Workspace.Create(DefaultOrganizationId, "Test", DefaultSlug);
            workspace.Suspend();

            workspace.Delete();

            workspace.Status.Should().Be(WorkspaceStatus.Deleted);
            workspace.DeletedAt.Should().NotBeNull();
        }

        [Fact]
        public void Delete_WhenAlreadyDeleted_ThrowsDomainException()
        {
            var workspace = Workspace.Create(DefaultOrganizationId, "Test", DefaultSlug);
            workspace.Delete();

            var act = () => workspace.Delete();

            act.Should().Throw<DomainException>()
                .WithMessage("*already deleted*");
        }

        [Fact]
        public void Delete_RaisesWorkspaceDeletedEvent()
        {
            var workspace = Workspace.Create(DefaultOrganizationId, "Test", DefaultSlug);
            workspace.ClearDomainEvents();

            workspace.Delete();

            workspace.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<WorkspaceDeleted>()
                .Which.Should().BeEquivalentTo(new
                {
                    WorkspaceId = workspace.Id,
                    OrganizationId = DefaultOrganizationId
                });
        }
    }

    public class AddMemberTests
    {
        [Fact]
        public void AddMember_WithValidData_AddsMemberAndReturns()
        {
            var workspace = Workspace.Create(DefaultOrganizationId, "Test", DefaultSlug);
            var userId = Guid.NewGuid();
            workspace.ClearDomainEvents();

            var membership = workspace.AddMember(userId, WorkspaceRole.Member);

            membership.Should().NotBeNull();
            membership.UserId.Should().Be(userId);
            membership.WorkspaceId.Should().Be(workspace.Id);
            membership.Role.Should().Be(WorkspaceRole.Member);
            membership.InvitedBy.Should().BeNull();
            workspace.Memberships.Should().ContainSingle().Which.Should().BeSameAs(membership);
        }

        [Fact]
        public void AddMember_WithInvitedBy_StoresInviter()
        {
            var workspace = Workspace.Create(DefaultOrganizationId, "Test", DefaultSlug);
            var userId = Guid.NewGuid();
            var inviterId = Guid.NewGuid();

            var membership = workspace.AddMember(userId, WorkspaceRole.Admin, inviterId);

            membership.InvitedBy.Should().Be(inviterId);
        }

        [Theory]
        [InlineData(WorkspaceRole.Owner)]
        [InlineData(WorkspaceRole.Admin)]
        [InlineData(WorkspaceRole.Member)]
        [InlineData(WorkspaceRole.Viewer)]
        public void AddMember_WithDifferentRoles_AssignsCorrectRole(WorkspaceRole role)
        {
            var workspace = Workspace.Create(DefaultOrganizationId, "Test", DefaultSlug);
            var userId = Guid.NewGuid();

            var membership = workspace.AddMember(userId, role);

            membership.Role.Should().Be(role);
        }

        [Fact]
        public void AddMember_WhenSuspended_ThrowsDomainException()
        {
            var workspace = Workspace.Create(DefaultOrganizationId, "Test", DefaultSlug);
            workspace.Suspend();

            var act = () => workspace.AddMember(Guid.NewGuid(), WorkspaceRole.Member);

            act.Should().Throw<DomainException>()
                .WithMessage("*inactive workspace*");
        }

        [Fact]
        public void AddMember_WhenDeleted_ThrowsDomainException()
        {
            var workspace = Workspace.Create(DefaultOrganizationId, "Test", DefaultSlug);
            workspace.Delete();

            var act = () => workspace.AddMember(Guid.NewGuid(), WorkspaceRole.Member);

            act.Should().Throw<DomainException>()
                .WithMessage("*inactive workspace*");
        }

        [Fact]
        public void AddMember_WhenUserAlreadyMember_ThrowsDomainException()
        {
            var workspace = Workspace.Create(DefaultOrganizationId, "Test", DefaultSlug);
            var userId = Guid.NewGuid();
            workspace.AddMember(userId, WorkspaceRole.Member);

            var act = () => workspace.AddMember(userId, WorkspaceRole.Admin);

            act.Should().Throw<DomainException>()
                .WithMessage("*already a member*");
        }

        [Fact]
        public void AddMember_RaisesMemberAddedEvent()
        {
            var workspace = Workspace.Create(DefaultOrganizationId, "Test", DefaultSlug);
            var userId = Guid.NewGuid();
            workspace.ClearDomainEvents();

            workspace.AddMember(userId, WorkspaceRole.Admin);

            workspace.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<MemberAdded>()
                .Which.Should().BeEquivalentTo(new
                {
                    WorkspaceId = workspace.Id,
                    UserId = userId,
                    Role = WorkspaceRole.Admin
                });
        }
    }

    public class RemoveMemberTests
    {
        [Fact]
        public void RemoveMember_WhenMemberExists_RemovesMember()
        {
            var workspace = Workspace.Create(DefaultOrganizationId, "Test", DefaultSlug);
            var userId = Guid.NewGuid();
            workspace.AddMember(userId, WorkspaceRole.Member);
            workspace.ClearDomainEvents();

            workspace.RemoveMember(userId);

            workspace.Memberships.Should().BeEmpty();
        }

        [Fact]
        public void RemoveMember_WhenMemberDoesNotExist_ThrowsDomainException()
        {
            var workspace = Workspace.Create(DefaultOrganizationId, "Test", DefaultSlug);
            var userId = Guid.NewGuid();

            var act = () => workspace.RemoveMember(userId);

            act.Should().Throw<DomainException>()
                .WithMessage("*not a member*");
        }

        [Fact]
        public void RemoveMember_RaisesMemberRemovedEvent()
        {
            var workspace = Workspace.Create(DefaultOrganizationId, "Test", DefaultSlug);
            var userId = Guid.NewGuid();
            workspace.AddMember(userId, WorkspaceRole.Member);
            workspace.ClearDomainEvents();

            workspace.RemoveMember(userId);

            workspace.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<MemberRemoved>()
                .Which.Should().BeEquivalentTo(new
                {
                    WorkspaceId = workspace.Id,
                    UserId = userId
                });
        }
    }

    public class ChangeMemberRoleTests
    {
        [Fact]
        public void ChangeMemberRole_WhenMemberExists_UpdatesRole()
        {
            var workspace = Workspace.Create(DefaultOrganizationId, "Test", DefaultSlug);
            var userId = Guid.NewGuid();
            workspace.AddMember(userId, WorkspaceRole.Member);
            workspace.ClearDomainEvents();

            workspace.ChangeMemberRole(userId, WorkspaceRole.Admin);

            workspace.Memberships.Should().ContainSingle()
                .Which.Role.Should().Be(WorkspaceRole.Admin);
        }

        [Fact]
        public void ChangeMemberRole_WhenMemberDoesNotExist_ThrowsDomainException()
        {
            var workspace = Workspace.Create(DefaultOrganizationId, "Test", DefaultSlug);
            var userId = Guid.NewGuid();

            var act = () => workspace.ChangeMemberRole(userId, WorkspaceRole.Admin);

            act.Should().Throw<DomainException>()
                .WithMessage("*not a member*");
        }

        [Fact]
        public void ChangeMemberRole_ToSameRole_DoesNotRaiseEvent()
        {
            var workspace = Workspace.Create(DefaultOrganizationId, "Test", DefaultSlug);
            var userId = Guid.NewGuid();
            workspace.AddMember(userId, WorkspaceRole.Member);
            workspace.ClearDomainEvents();

            workspace.ChangeMemberRole(userId, WorkspaceRole.Member);

            workspace.DomainEvents.Should().BeEmpty();
        }

        [Fact]
        public void ChangeMemberRole_ToDifferentRole_RaisesMemberRoleChangedEvent()
        {
            var workspace = Workspace.Create(DefaultOrganizationId, "Test", DefaultSlug);
            var userId = Guid.NewGuid();
            workspace.AddMember(userId, WorkspaceRole.Member);
            workspace.ClearDomainEvents();

            workspace.ChangeMemberRole(userId, WorkspaceRole.Admin);

            workspace.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<MemberRoleChanged>()
                .Which.Should().BeEquivalentTo(new
                {
                    WorkspaceId = workspace.Id,
                    UserId = userId,
                    OldRole = WorkspaceRole.Member,
                    NewRole = WorkspaceRole.Admin
                });
        }
    }

    public class DomainEventsTests
    {
        [Fact]
        public void ClearDomainEvents_RemovesAllEvents()
        {
            var workspace = Workspace.Create(DefaultOrganizationId, "Test", DefaultSlug);
            workspace.DomainEvents.Should().NotBeEmpty();

            workspace.ClearDomainEvents();

            workspace.DomainEvents.Should().BeEmpty();
        }

        [Fact]
        public void MultipleOperations_AccumulateEvents()
        {
            var workspace = Workspace.Create(DefaultOrganizationId, "Test", DefaultSlug);
            workspace.Update("Updated");
            workspace.AddMember(Guid.NewGuid(), WorkspaceRole.Member);
            workspace.Delete();

            workspace.DomainEvents.Should().HaveCount(4);
            workspace.DomainEvents.Select(e => e.GetType()).Should().ContainInOrder(
                typeof(WorkspaceCreated),
                typeof(WorkspaceUpdated),
                typeof(MemberAdded),
                typeof(WorkspaceDeleted));
        }
    }
}
