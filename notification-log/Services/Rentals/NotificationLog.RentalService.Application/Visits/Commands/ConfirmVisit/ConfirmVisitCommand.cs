namespace NotificationLog.RentalService.Application.Visits.Commands.ConfirmVisit;

public sealed record ConfirmVisitCommand(Guid ActorId, Guid VisitId, DateTimeOffset SlotStart);
