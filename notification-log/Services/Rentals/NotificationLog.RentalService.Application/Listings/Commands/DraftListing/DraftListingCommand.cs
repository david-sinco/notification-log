using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Application.Listings.Commands.DraftListing;

public sealed record DraftListingCommand(Guid OwnerId, Operation Operation);
