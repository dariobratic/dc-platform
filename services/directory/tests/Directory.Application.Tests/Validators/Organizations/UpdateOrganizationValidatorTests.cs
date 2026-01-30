using Directory.Application.Commands.Organizations;
using Directory.Application.Validators.Organizations;
using FluentValidation.TestHelper;

namespace Directory.Application.Tests.Validators.Organizations;

public class UpdateOrganizationValidatorTests
{
    private readonly UpdateOrganizationValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        // Arrange
        var command = new UpdateOrganizationCommand(Guid.NewGuid(), "Updated Name", null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyId_HasError()
    {
        // Arrange
        var command = new UpdateOrganizationCommand(Guid.Empty, "Valid Name", null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyName_HasError(string? name)
    {
        // Arrange
        var command = new UpdateOrganizationCommand(Guid.NewGuid(), name!, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WithNameExceeding200Chars_HasError()
    {
        // Arrange
        var command = new UpdateOrganizationCommand(Guid.NewGuid(), new string('x', 201), null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }
}
