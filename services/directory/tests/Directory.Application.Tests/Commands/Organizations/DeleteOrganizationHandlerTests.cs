using Directory.Application.Commands.Organizations;
using Directory.Application.Exceptions;
using Directory.Application.Interfaces;
using Directory.Domain.Entities;
using Directory.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace Directory.Application.Tests.Commands.Organizations;

public class DeleteOrganizationHandlerTests
{
    private readonly IOrganizationRepository _repository = Substitute.For<IOrganizationRepository>();
    private readonly DeleteOrganizationHandler _handler;

    public DeleteOrganizationHandlerTests()
    {
        _handler = new DeleteOrganizationHandler(_repository);
    }

    [Fact]
    public async Task Handle_WithExistingOrganization_DeletesAndSaves()
    {
        // Arrange
        var org = Organization.Create("To Delete", Slug.Create("delete-test"));
        _repository.GetByIdAsync(org.Id, Arg.Any<CancellationToken>()).Returns(org);

        var command = new DeleteOrganizationCommand(org.Id);

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        org.Status.Should().Be(OrganizationStatus.Deleted);
        org.DeletedAt.Should().NotBeNull();
        await _repository.Received(1).UpdateAsync(org, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ThrowsNotFoundException()
    {
        // Arrange
        var command = new DeleteOrganizationCommand(Guid.NewGuid());
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Organization?)null);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
