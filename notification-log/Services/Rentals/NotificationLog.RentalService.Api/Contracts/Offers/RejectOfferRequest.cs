namespace NotificationLog.RentalService.Api.Contracts.Offers;

public sealed record RejectOfferRequest(Guid ActorId, string Reason);
