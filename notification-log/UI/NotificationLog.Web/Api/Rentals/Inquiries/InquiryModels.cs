namespace NotificationLog.Web.Api.Rentals.Inquiries;

public sealed record InquirySummaryDto(
    Guid Id,
    Guid ListingId,
    Guid SeekerId,
    bool IsClosed,
    int MessageCount,
    string LastMessage,
    DateTimeOffset LastMessageAt);

public sealed record InquiryDto(
    Guid Id,
    Guid ListingId,
    Guid SeekerId,
    bool IsClosed,
    string? ClosedReason,
    DateTimeOffset OpenedAt,
    IReadOnlyList<InquiryMessageDto> Messages);

public sealed record InquiryMessageDto(Guid AuthorId, string Text, DateTimeOffset At);

public sealed record SendInquiryMessageRequest(Guid SeekerId, Guid ListingId, string Message);

public sealed record SentInquiryMessageResponse(Guid InquiryId);

public sealed record ReplyToInquiryRequest(Guid ActorId, string Message);

public static class InquiryLimits
{
    public const int MaxMessageLength = 1000;
}
