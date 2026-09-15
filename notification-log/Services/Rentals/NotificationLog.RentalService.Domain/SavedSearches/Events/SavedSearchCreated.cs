using Domain.Shared.EventSourcing;
using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Domain.SavedSearches.Events;

public sealed record SavedSearchCreated(
    Guid SearchId,
    string Name,
    Operation Operation,
    string City,
    IReadOnlyList<string> Neighborhoods,
    long? MinPrice,
    long? MaxPrice,
    int? MinBedrooms,
    int? MinStratum,
    int? MaxStratum,
    decimal? MinArea,
    AlertFrequency Frequency
) : DomainEvent;
