namespace NotificationLog.NotificationService.Domain.Common;

public abstract record DomainEvent: IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
}