using Directory.Application.Commands.Memberships;
using FluentValidation;

namespace Directory.Application.Validators.Memberships;

public sealed class RemoveMemberValidator : AbstractValidator<RemoveMemberCommand>
{
    public RemoveMemberValidator()
    {
        RuleFor(x => x.WorkspaceId)
            .NotEmpty().WithMessage("Workspace ID is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required.");
    }
}
