namespace NotificationLog.RentalService.Application.Inquiries.Commands.SendInquiryMessage;

public sealed record SendInquiryMessageCommand(Guid SeekerId, Guid ListingId, string Message);
