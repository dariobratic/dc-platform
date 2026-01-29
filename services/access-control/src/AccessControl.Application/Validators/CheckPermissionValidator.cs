using AccessControl.Application.Queries.Permissions.CheckPermission;
using FluentValidation;

namespace AccessControl.Application.Validators;

public class CheckPermissionValidator : AbstractValidator<CheckPermissionQuery>
{
    public CheckPermissionValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty();

        RuleFor(x => x.ScopeId)
            .NotEmpty();

        RuleFor(x => x.Permission)
            .NotEmpty()
            .MaximumLength(200);
    }
}
