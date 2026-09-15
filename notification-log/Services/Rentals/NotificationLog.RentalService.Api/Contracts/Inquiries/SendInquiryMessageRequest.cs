namespace NotificationLog.RentalService.Api.Contracts.Inquiries;

public sealed record SendInquiryMessageRequest(Guid SeekerId, Guid ListingId, string Message);
