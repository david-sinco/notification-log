namespace NotificationLog.RentalService.Api.Contracts.Listings;

public sealed record CancelReservationRequest(Guid ActorId, string Reason);
