using Domain.Shared.EventSourcing;

namespace NotificationLog.RentalService.Infrastructure.Persistence.Projections;

internal static class EventTime
{
    public static DateTimeOffset Of(IDomainEvent domainEvent)
        => new(DateTime.SpecifyKind(domainEvent.OccurredOn, DateTimeKind.Utc));
}
