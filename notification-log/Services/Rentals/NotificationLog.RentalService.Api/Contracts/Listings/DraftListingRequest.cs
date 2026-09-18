using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Api.Contracts.Listings;

public sealed record DraftListingRequest(Guid OwnerId, Operation Operation);
