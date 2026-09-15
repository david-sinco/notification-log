namespace NotificationLog.RentalService.Application.Offers.Commands.WithdrawOffer;

public sealed record WithdrawOfferCommand(Guid ActorId, Guid OfferId);
