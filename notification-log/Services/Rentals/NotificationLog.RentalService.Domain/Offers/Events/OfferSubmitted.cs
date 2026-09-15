using Domain.Shared.EventSourcing;
using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Domain.Offers.Events;

public sealed record OfferSubmitted(
    Guid OfferId,
    Guid ListingId,
    Guid OffererId,
    Operation Operation,
    long ListedPrice,
    long Amount,
    DateOnly? RentStartDate,
    int? RentTermMonths,
    DateTimeOffset RespondBy
) : DomainEvent;
