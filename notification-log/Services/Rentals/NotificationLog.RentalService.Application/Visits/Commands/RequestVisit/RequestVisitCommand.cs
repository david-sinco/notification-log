namespace NotificationLog.RentalService.Application.Visits.Commands.RequestVisit;

public sealed record RequestVisitCommand(Guid VisitorId, Guid ListingId, IReadOnlyList<DateTimeOffset> SlotStarts);
