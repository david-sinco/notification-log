namespace NotificationLog.RentalService.Api.Contracts.Visits;

public sealed record RequestVisitRequest(Guid VisitorId, Guid ListingId, IReadOnlyList<DateTimeOffset> SlotStarts);
