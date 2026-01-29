using Directory.Domain.Entities;
using Directory.Domain.Events;
using Directory.Domain.Exceptions;
using Directory.Domain.ValueObjects;
using FluentAssertions;

namespace Directory.Domain.Tests.Entities;

public class OrganizationTests
{
    private static readonly Slug DefaultSlug = Slug.Create("test-org");

    public class CreateTests
    {
        [Fact]
        public void Create_WithValidData_SetsProperties()
        {
            var org = Organization.Create("Test Org", DefaultSlug);

            org.Id.Should().NotBeEmpty();
            org.Name.Should().Be("Test Org");
            org.Slug.Should().Be(DefaultSlug);
            org.Status.Should().Be(OrganizationStatus.Active);
            org.Settings.Values.Should().BeEmpty();
            org.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
            org.UpdatedAt.Should().BeNull();
            org.DeletedAt.Should().BeNull();
        }

        [Fact]
        public void Create_WithSettings_StoresSettings()
        {
            var settings = OrganizationSettings.Create(new Dictionary<string, string>
            {
                ["theme"] = "dark",
                ["language"] = "en"
            });

            var org = Organization.Create("Test Org", DefaultSlug, settings);

            org.Settings.GetSetting("theme").Should().Be("dark");
            org.Settings.GetSetting("language").Should().Be("en");
        }

        [Fact]
        public void Create_TrimsName()
        {
            var org = Organization.Create("  Test Org  ", DefaultSlug);

            org.Name.Should().Be("Test Org");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithEmptyName_ThrowsDomainException(string? name)
        {
            var act = () => Organization.Create(name!, DefaultSlug);

            act.Should().Throw<DomainException>()
                .WithMessage("*name*empty*");
        }

        [Fact]
        public void Create_RaisesOrganizationCreatedEvent()
        {
            var org = Organization.Create("Test Org", DefaultSlug);

            org.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<OrganizationCreated>()
                .Which.Should().BeEquivalentTo(new
                {
                    OrganizationId = org.Id,
                    Name = "Test Org",
                    Slug = "test-org"
                });
        }
    }

    public class UpdateTests
    {
        [Fact]
        public void Update_WithValidName_UpdatesNameAndTimestamp()
        {
            var org = Organization.Create("Original", DefaultSlug);
            org.ClearDomainEvents();

            org.Update("Updated Name");

            org.Name.Should().Be("Updated Name");
            org.UpdatedAt.Should().NotBeNull();
        }

        [Fact]
        public void Update_WithSettings_UpdatesSettings()
        {
            var org = Organization.Create("Test", DefaultSlug);
            var newSettings = OrganizationSettings.Create(new Dictionary<string, string> { ["key"] = "value" });

            org.Update("Test", newSettings);

            org.Settings.GetSetting("key").Should().Be("value");
        }

        [Fact]
        public void Update_WithNullSettings_KeepsExistingSettings()
        {
            var original = OrganizationSettings.Create(new Dictionary<string, string> { ["key"] = "value" });
            var org = Organization.Create("Test", DefaultSlug, original);

            org.Update("New Name");

            org.Settings.GetSetting("key").Should().Be("value");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Update_WithEmptyName_ThrowsDomainException(string? name)
        {
            var org = Organization.Create("Test", DefaultSlug);

            var act = () => org.Update(name!);

            act.Should().Throw<DomainException>()
                .WithMessage("*name*empty*");
        }

        [Fact]
        public void Update_RaisesOrganizationUpdatedEvent()
        {
            var org = Organization.Create("Test", DefaultSlug);
            org.ClearDomainEvents();

            org.Update("Updated");

            org.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<OrganizationUpdated>()
                .Which.Should().BeEquivalentTo(new
                {
                    OrganizationId = org.Id,
                    Name = "Updated"
                });
        }
    }

    public class SuspendTests
    {
        [Fact]
        public void Suspend_FromActive_SetsSuspendedStatus()
        {
            var org = Organization.Create("Test", DefaultSlug);

            org.Suspend();

            org.Status.Should().Be(OrganizationStatus.Suspended);
            org.UpdatedAt.Should().NotBeNull();
        }

        [Fact]
        public void Suspend_WhenDeleted_ThrowsDomainException()
        {
            var org = Organization.Create("Test", DefaultSlug);
            org.Delete();

            var act = () => org.Suspend();

            act.Should().Throw<DomainException>()
                .WithMessage("*suspend*deleted*");
        }
    }

    public class ActivateTests
    {
        [Fact]
        public void Activate_FromSuspended_SetsActiveStatus()
        {
            var org = Organization.Create("Test", DefaultSlug);
            org.Suspend();

            org.Activate();

            org.Status.Should().Be(OrganizationStatus.Active);
        }

        [Fact]
        public void Activate_WhenDeleted_ThrowsDomainException()
        {
            var org = Organization.Create("Test", DefaultSlug);
            org.Delete();

            var act = () => org.Activate();

            act.Should().Throw<DomainException>()
                .WithMessage("*activate*deleted*");
        }
    }

    public class DeleteTests
    {
        [Fact]
        public void Delete_FromActive_SetsDeletedStatusAndTimestamp()
        {
            var org = Organization.Create("Test", DefaultSlug);

            org.Delete();

            org.Status.Should().Be(OrganizationStatus.Deleted);
            org.DeletedAt.Should().NotBeNull();
            org.UpdatedAt.Should().NotBeNull();
        }

        [Fact]
        public void Delete_FromSuspended_SetsDeletedStatus()
        {
            var org = Organization.Create("Test", DefaultSlug);
            org.Suspend();

            org.Delete();

            org.Status.Should().Be(OrganizationStatus.Deleted);
            org.DeletedAt.Should().NotBeNull();
        }

        [Fact]
        public void Delete_WhenAlreadyDeleted_ThrowsDomainException()
        {
            var org = Organization.Create("Test", DefaultSlug);
            org.Delete();

            var act = () => org.Delete();

            act.Should().Throw<DomainException>()
                .WithMessage("*already deleted*");
        }

        [Fact]
        public void Delete_RaisesOrganizationDeletedEvent()
        {
            var org = Organization.Create("Test", DefaultSlug);
            org.ClearDomainEvents();

            org.Delete();

            org.DomainEvents.Should().ContainSingle()
                .Which.Should().BeOfType<OrganizationDeleted>()
                .Which.OrganizationId.Should().Be(org.Id);
        }
    }

    public class DomainEventsTests
    {
        [Fact]
        public void ClearDomainEvents_RemovesAllEvents()
        {
            var org = Organization.Create("Test", DefaultSlug);
            org.DomainEvents.Should().NotBeEmpty();

            org.ClearDomainEvents();

            org.DomainEvents.Should().BeEmpty();
        }

        [Fact]
        public void MultipleOperations_AccumulateEvents()
        {
            var org = Organization.Create("Test", DefaultSlug);
            org.Update("Updated");
            org.Delete();

            org.DomainEvents.Should().HaveCount(3);
            org.DomainEvents.Select(e => e.GetType()).Should().ContainInOrder(
                typeof(OrganizationCreated),
                typeof(OrganizationUpdated),
                typeof(OrganizationDeleted));
        }
    }
}
