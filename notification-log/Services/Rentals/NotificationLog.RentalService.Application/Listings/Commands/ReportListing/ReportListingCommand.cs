using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.Application.Listings.Commands.ReportListing;

public sealed record ReportListingCommand(Guid ReporterId, Guid ListingId, ReportReason Reason);
