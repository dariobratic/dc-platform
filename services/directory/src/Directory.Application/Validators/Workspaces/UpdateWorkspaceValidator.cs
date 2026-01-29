using Directory.Application.Commands.Workspaces;
using FluentValidation;

namespace Directory.Application.Validators.Workspaces;

public sealed class UpdateWorkspaceValidator : AbstractValidator<UpdateWorkspaceCommand>
{
    public UpdateWorkspaceValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Workspace ID is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Workspace name is required.")
            .MaximumLength(200).WithMessage("Workspace name must not exceed 200 characters.");
    }
}
