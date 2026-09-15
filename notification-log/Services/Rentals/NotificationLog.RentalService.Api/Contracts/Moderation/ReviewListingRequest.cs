using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.Api.Contracts.Moderation;

public sealed record ReviewListingRequest(Guid ModeratorId, bool Approve, IReadOnlyList<RejectionReason> Reasons);
