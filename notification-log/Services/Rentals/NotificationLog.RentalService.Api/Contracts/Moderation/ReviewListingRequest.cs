using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.Api.Contracts.Moderation;

public sealed record ReviewListingRequest(bool Approve, IReadOnlyList<RejectionReason> Reasons);
