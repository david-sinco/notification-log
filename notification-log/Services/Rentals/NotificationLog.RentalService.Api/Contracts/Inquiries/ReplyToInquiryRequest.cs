namespace NotificationLog.RentalService.Api.Contracts.Inquiries;

public sealed record ReplyToInquiryRequest(Guid ActorId, string Message);
