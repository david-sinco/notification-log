namespace NotificationLog.RentalService.Api.Contracts.Visits;

public sealed record ConfirmVisitRequest(Guid ActorId, DateTimeOffset SlotStart);
