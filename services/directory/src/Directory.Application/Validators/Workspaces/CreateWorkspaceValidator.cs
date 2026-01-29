using Directory.Application.Commands.Workspaces;
using FluentValidation;

namespace Directory.Application.Validators.Workspaces;

public sealed class CreateWorkspaceValidator : AbstractValidator<CreateWorkspaceCommand>
{
    public CreateWorkspaceValidator()
    {
        RuleFor(x => x.OrganizationId)
            .NotEmpty().WithMessage("Organization ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Workspace name is required.")
            .MaximumLength(200).WithMessage("Workspace name must not exceed 200 characters.");

        RuleFor(x => x.Slug)
            .NotEmpty().WithMessage("Slug is required.")
            .MinimumLength(2).WithMessage("Slug must be at least 2 characters.")
            .MaximumLength(100).WithMessage("Slug must not exceed 100 characters.")
            .Matches(@"^[a-z0-9]([a-z0-9-]*[a-z0-9])?$")
            .WithMessage("Slug must contain only lowercase letters, numbers, and hyphens, and cannot start or end with a hyphen.");
    }
}
