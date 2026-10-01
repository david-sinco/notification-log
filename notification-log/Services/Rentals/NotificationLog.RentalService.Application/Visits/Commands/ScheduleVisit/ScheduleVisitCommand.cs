namespace NotificationLog.RentalService.Application.Visits.Commands.ScheduleVisit;

public sealed record ScheduleVisitCommand(Guid VisitId, DateTimeOffset StartsAt);
