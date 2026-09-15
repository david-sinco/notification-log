namespace NotificationLog.RentalService.Application.Offers.Dtos;

public sealed record OfferDto(
    Guid Id,
    Guid ListingId,
    Guid OffererId,
    string Operation,
    string Status,
    long ListedPrice,
    long Amount,
    DateOnly? RentStartDate,
    int? RentTermMonths,
    string LastMoveBy,
    int CounterOfferCount,
    DateTimeOffset RespondBy,
    DateTimeOffset SubmittedAt,
    DateTimeOffset UpdatedAt,
    IReadOnlyList<OfferMoveDto> History);
