using Domain.Shared.EventSourcing;

namespace NotificationLog.RentalService.Domain.Listings.Events;

public sealed record ListingClosed(
    Guid OfferId,
    long FinalPrice,
    DateOnly SignedOn
) : DomainEvent;
