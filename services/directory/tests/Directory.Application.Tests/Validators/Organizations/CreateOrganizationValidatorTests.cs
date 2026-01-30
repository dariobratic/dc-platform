using Directory.Application.Commands.Organizations;
using Directory.Application.Validators.Organizations;
using FluentAssertions;
using FluentValidation.TestHelper;

namespace Directory.Application.Tests.Validators.Organizations;

public class CreateOrganizationValidatorTests
{
    private readonly CreateOrganizationValidator _validator = new();

    [Fact]
    public void Validate_WithValidCommand_HasNoErrors()
    {
        // Arrange
        var command = new CreateOrganizationCommand("Valid Org", "valid-org", null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptyName_HasError(string? name)
    {
        // Arrange
        var command = new CreateOrganizationCommand(name!, "valid-slug", null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Validate_WithNameExceeding200Chars_HasError()
    {
        // Arrange
        var command = new CreateOrganizationCommand(new string('x', 201), "valid-slug", null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithEmptySlug_HasError(string? slug)
    {
        // Arrange
        var command = new CreateOrganizationCommand("Valid Name", slug!, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Slug);
    }

    [Fact]
    public void Validate_WithSlugTooShort_HasError()
    {
        // Arrange
        var command = new CreateOrganizationCommand("Valid Name", "a", null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Slug);
    }

    [Theory]
    [InlineData("UPPERCASE")]
    [InlineData("has spaces")]
    [InlineData("special!chars")]
    [InlineData("-starts-with-hyphen")]
    [InlineData("ends-with-hyphen-")]
    public void Validate_WithInvalidSlugFormat_HasError(string slug)
    {
        // Arrange
        var command = new CreateOrganizationCommand("Valid Name", slug, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Slug);
    }

    [Theory]
    [InlineData("valid-slug")]
    [InlineData("my-org-123")]
    [InlineData("ab")]
    [InlineData("a1")]
    public void Validate_WithValidSlugFormat_HasNoError(string slug)
    {
        // Arrange
        var command = new CreateOrganizationCommand("Valid Name", slug, null);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Slug);
    }
}
