using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.Api.Contracts.Listings;

public sealed record DraftListingRequest(Guid OwnerId, Operation Operation);
