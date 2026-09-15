namespace NotificationLog.RentalService.Api.Contracts.Visits;

public sealed record ReportVisitOutcomeRequest(Guid ActorId, bool Attended);
