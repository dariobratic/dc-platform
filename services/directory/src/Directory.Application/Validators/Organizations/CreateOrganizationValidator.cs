using Directory.Application.Commands.Organizations;
using FluentValidation;

namespace Directory.Application.Validators.Organizations;

public sealed class CreateOrganizationValidator : AbstractValidator<CreateOrganizationCommand>
{
    public CreateOrganizationValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Organization name is required.")
            .MaximumLength(200).WithMessage("Organization name must not exceed 200 characters.");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required.")
            .MinimumLength(2).WithMessage("Slug must be at least 2 characters.")
            .MaximumLength(100).WithMessage("Slug must not exceed 100 characters.")
            .Matches(@"^[a-z0-9]([a-z0-9-]*[a-z0-9])?$")
            .WithMessage("Slug must contain only lowercase letters, numbers, and hyphens, and cannot start or end with a hyphen.");
    }
}
