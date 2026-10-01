namespace NotificationLog.RentalService.Application.Visits.Commands.CancelVisit;

public sealed record CancelVisitCommand(Guid VisitId, string Reason);
