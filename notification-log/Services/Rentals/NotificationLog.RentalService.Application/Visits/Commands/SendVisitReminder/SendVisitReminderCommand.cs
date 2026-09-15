namespace NotificationLog.RentalService.Application.Visits.Commands.SendVisitReminder;

public sealed record SendVisitReminderCommand(Guid VisitId, DateTimeOffset SlotStart);
