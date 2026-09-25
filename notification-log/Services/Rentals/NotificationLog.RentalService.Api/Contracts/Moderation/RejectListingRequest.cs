using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.Api.Contracts.Moderation;

public sealed record RejectListingRequest(IReadOnlyList<RejectionReason> Reasons);
