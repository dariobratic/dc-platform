using AccessControl.Application.Commands.Roles.CreateRole;
using FluentValidation;

namespace AccessControl.Application.Validators;

public class CreateRoleValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.ScopeId)
            .NotEmpty();

        RuleFor(x => x.ScopeType)
            .IsInEnum();

        RuleFor(x => x.Permissions)
            .NotNull();

        RuleForEach(x => x.Permissions)
            .NotEmpty()
            .MaximumLength(200)
            .Matches(@"^[a-z][a-z0-9_]*:[a-z][a-z0-9_]*$")
            .WithMessage("Permission must be in format 'resource:action' (e.g., 'document:read').");
    }
}
