namespace NotificationLog.RentalService.Application.Inquiries.Dtos;

public sealed record InquiryMessageDto(Guid AuthorId, string Text, DateTimeOffset At);
