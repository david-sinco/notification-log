using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Application.Common;

namespace NotificationLog.RentalService.Application.Inquiries.Queries.ListInquiries;

public sealed class ListInquiriesHandler
{
    private readonly IInquiryReadModel _inquiries;

    public ListInquiriesHandler(IInquiryReadModel inquiries) => _inquiries = inquiries;

    public async Task<PagedResult<InquirySummaryDto>> HandleAsync(ListInquiriesQuery query, CancellationToken ct)
    {
        var paged = query with { Page = Paging.Page(query.Page), PageSize = Paging.Size(query.PageSize) };
        var (items, total) = await _inquiries.ListAsync(paged, ct);

        return new PagedResult<InquirySummaryDto>(items, paged.Page, paged.PageSize, total);
    }
}
