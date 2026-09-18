namespace NotificationLog.Web.Api.Rentals.Offers;

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

public sealed record OfferMoveDto(string Kind, string? By, long? Amount, string? Reason, DateTimeOffset At);

public sealed record SubmitOfferRequest(Guid OffererId, Guid ListingId, long Amount, DateOnly? RentStartDate, int? RentTermMonths);

public sealed record CounterOfferRequest(Guid ActorId, long Amount);

public sealed record OfferActorRequest(Guid ActorId);

public sealed record RejectOfferRequest(Guid ActorId, string Reason);

public static class OfferLimits
{
    public const int MaxCounterOffers = 3;
    public const int MinRentMonths = 6;
    public const int MaxRentMonths = 36;
}
