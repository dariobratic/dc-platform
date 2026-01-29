using Directory.Application.Commands.Memberships;
using FluentValidation;

namespace Directory.Application.Validators.Memberships;

public sealed class ChangeMemberRoleValidator : AbstractValidator<ChangeMemberRoleCommand>
{
    public ChangeMemberRoleValidator()
    {
        RuleFor(x => x.WorkspaceId)
            .NotEmpty().WithMessage("Workspace ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Valid workspace role is required.");
    }
}
