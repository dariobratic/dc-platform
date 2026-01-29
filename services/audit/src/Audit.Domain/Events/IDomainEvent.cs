using MediatR;

namespace Audit.Domain.Events;

public interface IDomainEvent : INotification
{
    DateTime OccurredAt { get; }
}
