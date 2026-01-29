using Audit.Application.Queries.GetAuditEntries;
using FluentValidation;

namespace Audit.Application.Validators;

public class GetAuditEntriesValidator : AbstractValidator<GetAuditEntriesQuery>
{
    public GetAuditEntriesValidator()
    {
        RuleFor(x => x.Skip)
            .GreaterThanOrEqualTo(0).WithMessage("Skip must be greater than or equal to 0.");

        RuleFor(x => x.Take)
            .GreaterThan(0).WithMessage("Take must be greater than 0.")
            .LessThanOrEqualTo(200).WithMessage("Take must not exceed 200.");

        RuleFor(x => x.EntityType)
            .MaximumLength(100).WithMessage("EntityType must not exceed 100 characters.")
            .When(x => x.EntityType is not null);

        RuleFor(x => x.Action)
            .MaximumLength(200).WithMessage("Action must not exceed 200 characters.")
            .When(x => x.Action is not null);

        RuleFor(x => x.ServiceName)
            .MaximumLength(100).WithMessage("ServiceName must not exceed 100 characters.")
            .When(x => x.ServiceName is not null);

        RuleFor(x => x)
            .Must(x => !x.From.HasValue || !x.To.HasValue || x.From.Value < x.To.Value)
            .WithMessage("From must be less than To when both are provided.");
    }
}
