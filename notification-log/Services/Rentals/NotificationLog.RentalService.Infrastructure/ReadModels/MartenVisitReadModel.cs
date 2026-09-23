using Application.Shared.Pagination;
using Marten;
using Marten.Linq;
using NotificationLog.RentalService.Application.Visits.Queries;
using NotificationLog.RentalService.Application.Visits.Queries.Dtos;
using NotificationLog.RentalService.Application.Visits.Queries.Filters;

namespace NotificationLog.RentalService.Infrastructure.ReadModels;

internal sealed class MartenVisitReadModel : IVisitReadModel
{
    private readonly IQuerySession _session;

    public MartenVisitReadModel(IQuerySession session) => _session = session;

    public async Task<(IReadOnlyList<VisitSummaryDto> Items, int TotalCount)> ListAsync(VisitFilter filter, PageRequest paging, CancellationToken ct)
    {
        IQueryable<VisitView> visits = _session.Query<VisitView>();

        if (filter.ListingId is { } listingId)
            visits = visits.Where(x => x.ListingId == listingId);

        if (filter.ParticipantId is { } participant)
            visits = visits.Where(x => x.VisitorId == participant || x.HostId == participant);

        if (filter.Status is { } status)
            visits = visits.Where(x => x.Status == status);

        var items = await ((IMartenQueryable<VisitView>)visits)
            .Stats(out QueryStatistics stats)
            .OrderByDescending(x => x.UpdatedAt)
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .ToListAsync(ct);

        return (items.Select(x => new VisitSummaryDto(
                x.Id,
                x.ListingId,
                x.VisitorId,
                x.HostId,
                x.Status.ToString(),
                x.FirstSlotStart,
                x.ConfirmedSlotStart,
                x.RespondBy,
                x.UpdatedAt))
            .ToList(), (int)stats.TotalResults);
    }

    public async Task<VisitDto?> GetAsync(Guid id, CancellationToken ct)
        => await _session.LoadAsync<VisitView>(id, ct) is { } x
            ? new VisitDto(
                x.Id,
                x.ListingId,
                x.VisitorId,
                x.HostId,
                x.Status.ToString(),
                x.SlotStarts,
                x.SlotDurationMinutes,
                x.ConfirmedSlotStart,
                x.RespondBy,
                x.CancelledBy?.ToString(),
                x.IsLateCancellation,
                x.Reason,
                x.RequestedAt,
                x.UpdatedAt)
            : null;
}
