namespace NotificationLog.RentalService.Application.Inquiries.Queries.ListInquiries;

public sealed record ListInquiriesQuery(
    Guid? ListingId = null,
    Guid? SeekerId = null,
    bool? IsClosed = null,
    int Page = 1,
    int PageSize = 20);
