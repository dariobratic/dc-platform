using Directory.Application.Commands.Organizations;
using Directory.Application.Exceptions;
using Directory.Application.Interfaces;
using Directory.Domain.Entities;
using FluentAssertions;
using NSubstitute;

namespace Directory.Application.Tests.Commands.Organizations;

public class CreateOrganizationHandlerTests
{
    private readonly IOrganizationRepository _repository = Substitute.For<IOrganizationRepository>();
    private readonly CreateOrganizationHandler _handler;

    public CreateOrganizationHandlerTests()
    {
        _handler = new CreateOrganizationHandler(_repository);
    }

    [Fact]
    public async Task Handle_WithValidCommand_CreatesOrganization()
    {
        // Arrange
        var command = new CreateOrganizationCommand("Test Org", "test-org", null);
        _repository.SlugExistsAsync("test-org", Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Test Org");
        result.Slug.Should().Be("test-org");
        await _repository.Received(1).AddAsync(Arg.Any<Organization>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithSettings_CreatesOrganizationWithSettings()
    {
        // Arrange
        var settings = new Dictionary<string, string> { ["theme"] = "dark", ["lang"] = "en" };
        var command = new CreateOrganizationCommand("Test Org", "test-org-settings", settings);
        _repository.SlugExistsAsync("test-org-settings", Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Settings.Should().ContainKey("theme");
        result.Settings["theme"].Should().Be("dark");
    }

    [Fact]
    public async Task Handle_WithDuplicateSlug_ThrowsConflictException()
    {
        // Arrange
        var command = new CreateOrganizationCommand("Test Org", "existing-slug", null);
        _repository.SlugExistsAsync("existing-slug", Arg.Any<CancellationToken>()).Returns(true);

        // Act
        var act = () => _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ConflictException>();
    }

    [Fact]
    public async Task Handle_ReturnsOrganizationResponse()
    {
        // Arrange
        var command = new CreateOrganizationCommand("Response Test", "response-test", null);
        _repository.SlugExistsAsync("response-test", Arg.Any<CancellationToken>()).Returns(false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Id.Should().NotBeEmpty();
        result.Name.Should().Be("Response Test");
        result.Slug.Should().Be("response-test");
        result.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
