using MediatR;

namespace Directory.Domain.Events;

public interface IDomainEvent : INotification
{
    DateTime OccurredAt { get; }
}
