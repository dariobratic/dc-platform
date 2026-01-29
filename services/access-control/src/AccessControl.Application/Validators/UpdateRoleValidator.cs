using AccessControl.Application.Commands.Roles.UpdateRole;
using FluentValidation;

namespace AccessControl.Application.Validators;

public class UpdateRoleValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Permissions)
            .NotNull();

        RuleForEach(x => x.Permissions)
            .NotEmpty()
            .MaximumLength(200)
            .Matches(@"^[a-z][a-z0-9_]*:[a-z][a-z0-9_]*$")
            .WithMessage("Permission must be in format 'resource:action' (e.g., 'document:read').");
    }
}
