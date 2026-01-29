using Audit.Application.Commands.CreateAuditEntry;
using FluentValidation;

namespace Audit.Application.Validators;

public class CreateAuditEntryValidator : AbstractValidator<CreateAuditEntryCommand>
{
    public CreateAuditEntryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.Action)
            .NotEmpty().WithMessage("Action is required.")
            .MaximumLength(200).WithMessage("Action must not exceed 200 characters.");

        RuleFor(x => x.EntityType)
            .NotEmpty().WithMessage("EntityType is required.")
            .MaximumLength(100).WithMessage("EntityType must not exceed 100 characters.");

        RuleFor(x => x.EntityId)
            .NotEmpty().WithMessage("EntityId is required.");

        RuleFor(x => x.ServiceName)
            .NotEmpty().WithMessage("ServiceName is required.")
            .MaximumLength(100).WithMessage("ServiceName must not exceed 100 characters.");

        RuleFor(x => x.UserEmail)
            .MaximumLength(320).WithMessage("UserEmail must not exceed 320 characters.")
            .When(x => x.UserEmail is not null);

        RuleFor(x => x.Details)
            .MaximumLength(10000).WithMessage("Details must not exceed 10000 characters.")
            .When(x => x.Details is not null);

        RuleFor(x => x.IpAddress)
            .MaximumLength(45).WithMessage("IpAddress must not exceed 45 characters.")
            .When(x => x.IpAddress is not null);

        RuleFor(x => x.UserAgent)
            .MaximumLength(500).WithMessage("UserAgent must not exceed 500 characters.")
            .When(x => x.UserAgent is not null);

        RuleFor(x => x.CorrelationId)
            .MaximumLength(100).WithMessage("CorrelationId must not exceed 100 characters.")
            .When(x => x.CorrelationId is not null);
    }
}
