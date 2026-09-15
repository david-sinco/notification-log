namespace NotificationLog.RentalService.Application.Listings.Commands.ExtendReservation;

public sealed record ExtendReservationCommand(Guid ActorId, Guid ListingId);
