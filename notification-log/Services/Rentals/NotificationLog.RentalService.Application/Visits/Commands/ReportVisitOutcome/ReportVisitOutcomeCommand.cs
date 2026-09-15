namespace NotificationLog.RentalService.Application.Visits.Commands.ReportVisitOutcome;

public sealed record ReportVisitOutcomeCommand(Guid ActorId, Guid VisitId, bool Attended);
