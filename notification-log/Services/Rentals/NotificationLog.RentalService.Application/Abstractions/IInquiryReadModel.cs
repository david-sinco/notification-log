using NotificationLog.RentalService.Application.Inquiries.Dtos;
using NotificationLog.RentalService.Application.Inquiries.Queries.ListInquiries;

namespace NotificationLog.RentalService.Application.Abstractions;

public interface IInquiryReadModel
{
    Task<(IReadOnlyList<InquirySummaryDto> Items, int TotalCount)> ListAsync(ListInquiriesQuery query, CancellationToken ct);

    Task<InquiryDto?> GetAsync(Guid id, CancellationToken ct);
}
