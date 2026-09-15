namespace NotificationLog.RentalService.Api.Contracts.Listings;

public sealed record WithdrawListingRequest(Guid ActorId, string Reason);
