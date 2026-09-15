using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.Api.Contracts.Listings;

public sealed record ReportListingRequest(Guid ReporterId, ReportReason Reason);
