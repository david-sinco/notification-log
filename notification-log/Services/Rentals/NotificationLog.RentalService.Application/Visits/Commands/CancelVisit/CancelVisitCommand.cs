namespace NotificationLog.RentalService.Application.Visits.Commands.CancelVisit;

public sealed record CancelVisitCommand(Guid ActorId, Guid VisitId, string Reason);
