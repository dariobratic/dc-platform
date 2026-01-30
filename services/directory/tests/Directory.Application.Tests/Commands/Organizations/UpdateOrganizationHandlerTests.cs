using Directory.Application.Commands.Organizations;
using Directory.Application.Exceptions;
using Directory.Application.Interfaces;
using Directory.Domain.Entities;
using Directory.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace Directory.Application.Tests.Commands.Organizations;

public class UpdateOrganizationHandlerTests
{
    private readonly IOrganizationRepository _repository = Substitute.For<IOrganizationRepository>();
    private readonly UpdateOrganizationHandler _handler;

    public UpdateOrganizationHandlerTests()
    {
        _handler = new UpdateOrganizationHandler(_repository);
    }

    [Fact]
    public async Task Handle_WithExistingOrganization_UpdatesAndSaves()
    {
        // Arrange
        var org = Organization.Create("Original", Slug.Create("update-test"));
        _repository.GetByIdAsync(org.Id, Arg.Any<CancellationToken>()).Returns(org);

        var command = new UpdateOrganizationCommand(org.Id, "Updated Name", null);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Name.Should().Be("Updated Name");
        await _repository.Received(1).UpdateAsync(Arg.Any<Organization>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithSettings_UpdatesSettings()
    {
        // Arrange
        var org = Organization.Create("Original", Slug.Create("update-settings"));
        _repository.GetByIdAsync(org.Id, Arg.Any<CancellationToken>()).Returns(org);

        var settings = new Dictionary<string, string> { ["key"] = "value" };
        var command = new UpdateOrganizationCommand(org.Id, "Original", settings);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Settings.Should().ContainKey("key");
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ThrowsNotFoundException()
    {
        // Arrange
        var command = new UpdateOrganizationCommand(Guid.NewGuid(), "Name", null);
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Organization?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
