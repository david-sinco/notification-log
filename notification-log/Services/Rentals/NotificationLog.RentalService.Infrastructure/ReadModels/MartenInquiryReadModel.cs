using Marten;
using Marten.Linq;
using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Application.Inquiries.Dtos;
using NotificationLog.RentalService.Application.Inquiries.Queries.ListInquiries;

namespace NotificationLog.RentalService.Infrastructure.ReadModels;

internal sealed class MartenInquiryReadModel : IInquiryReadModel
{
    private readonly IQuerySession _session;

    public MartenInquiryReadModel(IQuerySession session) => _session = session;

    public async Task<(IReadOnlyList<InquirySummaryDto> Items, int TotalCount)> ListAsync(ListInquiriesQuery query, CancellationToken ct)
    {
        IQueryable<InquiryView> inquiries = _session.Query<InquiryView>();

        if (query.ListingId is { } listingId)
            inquiries = inquiries.Where(x => x.ListingId == listingId);

        if (query.SeekerId is { } seekerId)
            inquiries = inquiries.Where(x => x.SeekerId == seekerId);

        if (query.IsClosed is { } isClosed)
            inquiries = inquiries.Where(x => x.IsClosed == isClosed);

        var items = await ((IMartenQueryable<InquiryView>)inquiries)
            .Stats(out QueryStatistics stats)
            .OrderByDescending(x => x.LastMessageAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return (items.Select(x => new InquirySummaryDto(
                x.Id,
                x.ListingId,
                x.SeekerId,
                x.IsClosed,
                x.Messages.Count,
                x.Messages.Count == 0 ? string.Empty : x.Messages[^1].Text,
                x.LastMessageAt))
            .ToList(), (int)stats.TotalResults);
    }

    public async Task<InquiryDto?> GetAsync(Guid id, CancellationToken ct)
        => await _session.LoadAsync<InquiryView>(id, ct) is { } x
            ? new InquiryDto(
                x.Id,
                x.ListingId,
                x.SeekerId,
                x.IsClosed,
                x.ClosedReason,
                x.OpenedAt,
                x.Messages.Select(m => new InquiryMessageDto(m.AuthorId, m.Text, m.At)).ToList())
            : null;
}
