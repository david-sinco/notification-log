namespace NotificationLog.RentalService.Api.Contracts.Visits;

public sealed record DeclineVisitRequest(Guid ActorId, string Reason);
