namespace NotificationLog.RentalService.Application.Visits.Commands.DeclineVisit;

public sealed record DeclineVisitCommand(Guid ActorId, Guid VisitId, string Reason);
