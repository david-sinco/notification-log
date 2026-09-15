namespace NotificationLog.RentalService.Api.Contracts.Offers;

public sealed record CounterOfferRequest(Guid ActorId, long Amount);
