namespace NotificationLog.RentalService.Application.Visits.Commands.RequestVisit;

public sealed record RequestVisitCommand(Guid ListingId, IReadOnlyList<DateTimeOffset> Slots);
