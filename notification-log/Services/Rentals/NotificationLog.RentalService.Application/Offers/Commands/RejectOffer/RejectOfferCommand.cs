namespace NotificationLog.RentalService.Application.Offers.Commands.RejectOffer;

public sealed record RejectOfferCommand(Guid ActorId, Guid OfferId, string Reason);
