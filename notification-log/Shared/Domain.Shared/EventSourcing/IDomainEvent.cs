namespace Domain.Shared.EventSourcing;

public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}