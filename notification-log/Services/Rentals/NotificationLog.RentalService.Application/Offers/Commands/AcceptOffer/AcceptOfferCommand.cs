namespace NotificationLog.RentalService.Application.Offers.Commands.AcceptOffer;

public sealed record AcceptOfferCommand(Guid ActorId, Guid OfferId);
