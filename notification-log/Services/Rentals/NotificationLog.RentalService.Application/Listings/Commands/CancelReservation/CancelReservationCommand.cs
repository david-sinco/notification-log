namespace NotificationLog.RentalService.Application.Listings.Commands.CancelReservation;

public sealed record CancelReservationCommand(Guid ActorId, Guid ListingId, string Reason);
