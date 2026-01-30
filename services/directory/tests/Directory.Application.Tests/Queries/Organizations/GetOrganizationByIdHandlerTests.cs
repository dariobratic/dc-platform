using Directory.Application.Exceptions;
using Directory.Application.Interfaces;
using Directory.Application.Queries.Organizations;
using Directory.Domain.Entities;
using Directory.Domain.ValueObjects;
using FluentAssertions;
using NSubstitute;

namespace Directory.Application.Tests.Queries.Organizations;

public class GetOrganizationByIdHandlerTests
{
    private readonly IOrganizationRepository _repository = Substitute.For<IOrganizationRepository>();
    private readonly GetOrganizationByIdHandler _handler;

    public GetOrganizationByIdHandlerTests()
    {
        _handler = new GetOrganizationByIdHandler(_repository);
    }

    [Fact]
    public async Task Handle_WithExistingId_ReturnsOrganizationResponse()
    {
        // Arrange
        var org = Organization.Create("Test Org", Slug.Create("get-test"));
        _repository.GetByIdAsync(org.Id, Arg.Any<CancellationToken>()).Returns(org);

        var query = new GetOrganizationByIdQuery(org.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().Be(org.Id);
        result.Name.Should().Be("Test Org");
        result.Slug.Should().Be("get-test");
    }

    [Fact]
    public async Task Handle_WithNonExistentId_ThrowsNotFoundException()
    {
        // Arrange
        var query = new GetOrganizationByIdQuery(Guid.NewGuid());
        _repository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns((Organization?)null);

        // Act
        var act = () => _handler.Handle(query, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>();
    }
}
