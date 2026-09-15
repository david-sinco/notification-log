using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.Application.Listings.Commands.ReviewListing;

public sealed record ReviewListingCommand(Guid ModeratorId, Guid ListingId, bool Approve, IReadOnlyList<RejectionReason> Reasons);
