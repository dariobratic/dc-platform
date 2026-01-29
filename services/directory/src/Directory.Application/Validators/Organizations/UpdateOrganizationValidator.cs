using Directory.Application.Commands.Organizations;
using FluentValidation;

namespace Directory.Application.Validators.Organizations;

public sealed class UpdateOrganizationValidator : AbstractValidator<UpdateOrganizationCommand>
{
    public UpdateOrganizationValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Organization ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Organization name is required.")
            .MaximumLength(200).WithMessage("Organization name must not exceed 200 characters.");
    }
}
