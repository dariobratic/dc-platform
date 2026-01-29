using Directory.Application.Commands.Invitations;
using FluentValidation;

namespace Directory.Application.Validators.Invitations;

public sealed class CreateInvitationValidator : AbstractValidator<CreateInvitationCommand>
{
    public CreateInvitationValidator()
    {
        RuleFor(x => x.WorkspaceId)
            .NotEmpty().WithMessage("Workspace ID is required.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Valid workspace role is required.");

        RuleFor(x => x.InvitedBy)
            .NotEmpty().WithMessage("InvitedBy user ID is required.");
    }
}
