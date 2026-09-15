namespace NotificationLog.RentalService.Application.Inquiries.Commands.CloseInquiry;

public sealed record CloseInquiryCommand(Guid InquiryId, string Reason);
