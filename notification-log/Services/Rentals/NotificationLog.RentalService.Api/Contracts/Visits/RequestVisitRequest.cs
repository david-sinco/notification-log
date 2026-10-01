namespace NotificationLog.RentalService.Api.Contracts.Visits;

public sealed record RequestVisitRequest(Guid ListingId, IReadOnlyList<DateTimeOffset> Slots);
