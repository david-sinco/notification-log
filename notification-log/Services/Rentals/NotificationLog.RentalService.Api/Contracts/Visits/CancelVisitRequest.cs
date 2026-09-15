namespace NotificationLog.RentalService.Api.Contracts.Visits;

public sealed record CancelVisitRequest(Guid ActorId, string Reason);
