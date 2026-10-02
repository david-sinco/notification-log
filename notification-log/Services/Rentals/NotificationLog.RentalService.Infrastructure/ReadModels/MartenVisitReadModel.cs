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

    public async Task<(IReadOnlyList<VisitDto> Items, int TotalCount)> ListAsync(VisitFilter filter, PageRequest paging, CancellationToken ct)
    {
        IQueryable<VisitView> visits = _session.Query<VisitView>();

        if (filter.Status is { } status)
            visits = visits.Where(x => x.Status == status);

        if (filter.ListingId is { } listingId)
            visits = visits.Where(x => x.ListingId == listingId);

        if (filter.ParticipantId is { } participant)
            visits = visits.Where(x => x.HostId == participant || x.VisitorId == participant);

        var items = await ((IMartenQueryable<VisitView>)visits)
            .Stats(out QueryStatistics stats)
            .OrderByDescending(x => x.UpdatedAt)
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .ToListAsync(ct);

        return (await ToDtosAsync(items, ct), (int)stats.TotalResults);
    }

    public async Task<VisitDto?> GetAsync(Guid id, CancellationToken ct)
        => await _session.LoadAsync<VisitView>(id, ct) is { } view ? (await ToDtosAsync([view], ct))[0] : null;

    private async Task<IReadOnlyList<VisitDto>> ToDtosAsync(IReadOnlyList<VisitView> visits, CancellationToken ct)
    {
        var listings = (await _session.LoadManyAsync<ListingView>(ct, visits.Select(x => x.ListingId).Distinct().ToArray()))
            .ToDictionary(x => x.Id);
        var visitors = (await _session.LoadManyAsync<VisitorView>(ct, visits.Select(x => x.VisitorId).Distinct().ToArray()))
            .ToDictionary(x => x.Id, x => x.DisplayName);
        var hosts = (await _session.LoadManyAsync<OwnerView>(ct, visits.Select(x => x.HostId).Distinct().ToArray()))
            .ToDictionary(x => x.Id, x => x.DisplayName());

        return visits.Select(x =>
        {
            var listing = listings.GetValueOrDefault(x.ListingId);

            return new VisitDto(
                x.Id,
                x.ListingId,
                x.HostId,
                x.VisitorId,
                x.Status.ToString(),
                x.ProposedSlots,
                x.RespondBy,
                x.ScheduledStartsAt,
                x.ScheduledEndsAt,
                x.CancelledBy?.ToString(),
                x.CancellationReason,
                x.IsLateCancellation,
                x.ClosedBy?.ToString(),
                x.RequestedAt,
                x.UpdatedAt,
                listing?.Type?.ToString(),
                listing?.Neighborhood,
                listing?.City,
                visitors.GetValueOrDefault(x.VisitorId),
                hosts.GetValueOrDefault(x.HostId));
        }).ToList();
    }
}
