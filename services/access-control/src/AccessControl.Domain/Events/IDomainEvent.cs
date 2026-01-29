using MediatR;

namespace AccessControl.Domain.Events;

public interface IDomainEvent : INotification
{
    DateTime OccurredAt { get; }
}
