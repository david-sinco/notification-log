namespace NotificationLog.RentalService.Application.Inquiries.Commands.ReplyToInquiry;

public sealed record ReplyToInquiryCommand(Guid ActorId, Guid InquiryId, string Message);
