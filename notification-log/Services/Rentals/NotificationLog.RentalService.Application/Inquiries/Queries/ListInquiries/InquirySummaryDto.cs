namespace NotificationLog.RentalService.Application.Inquiries.Queries.ListInquiries;

public sealed record InquirySummaryDto(
    Guid Id,
    Guid ListingId,
    Guid SeekerId,
    bool IsClosed,
    int MessageCount,
    string LastMessage,
    DateTimeOffset LastMessageAt);
