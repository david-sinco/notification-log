using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.Application.Listings.Commands.DraftListing;

public sealed record DraftListingCommand(Guid OwnerId, Operation Operation);
