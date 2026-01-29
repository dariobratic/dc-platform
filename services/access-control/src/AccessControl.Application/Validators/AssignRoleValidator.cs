using AccessControl.Application.Commands.RoleAssignments.AssignRole;
using FluentValidation;

namespace AccessControl.Application.Validators;

public class AssignRoleValidator : AbstractValidator<AssignRoleCommand>
{
    public AssignRoleValidator()
    {
        RuleFor(x => x.RoleId)
            .NotEmpty();

        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.ScopeId)
            .NotEmpty();

        RuleFor(x => x.ScopeType)
            .IsInEnum();

        RuleFor(x => x.AssignedBy)
            .NotEmpty();
    }
}
