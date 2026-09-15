namespace NotificationLog.RentalService.Application.Inquiries.Dtos;

public sealed record InquiryDto(
    Guid Id,
    Guid ListingId,
    Guid SeekerId,
    bool IsClosed,
    string? ClosedReason,
    DateTimeOffset OpenedAt,
    IReadOnlyList<InquiryMessageDto> Messages);
