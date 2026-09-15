namespace NotificationLog.RentalService.Application.Offers.Queries.ListOffers;

public sealed record OfferSummaryDto(
    Guid Id,
    Guid ListingId,
    Guid OffererId,
    string Operation,
    string Status,
    long ListedPrice,
    long Amount,
    string LastMoveBy,
    int CounterOfferCount,
    DateTimeOffset RespondBy,
    DateTimeOffset UpdatedAt);
