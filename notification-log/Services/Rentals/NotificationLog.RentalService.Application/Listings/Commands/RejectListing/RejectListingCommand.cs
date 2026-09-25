using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.Application.Listings.Commands.RejectListing;

public sealed record RejectListingCommand(Guid ListingId, IReadOnlyList<RejectionReason> Reasons);
