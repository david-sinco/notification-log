namespace NotificationLog.RentalService.Application.Offers.Commands.CounterOffer;

public sealed record CounterOfferCommand(Guid ActorId, Guid OfferId, long Amount);
