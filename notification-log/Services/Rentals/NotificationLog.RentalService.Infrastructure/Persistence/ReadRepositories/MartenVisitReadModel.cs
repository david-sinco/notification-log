using Application.Shared.Pagination;
using Marten;
using Marten.Linq;
using NotificationLog.RentalService.Application.Visits.Queries;
using NotificationLog.RentalService.Application.Visits.Queries.Dtos;
using NotificationLog.RentalService.Application.Visits.Queries.Filters;
using NotificationLog.RentalService.Infrastructure.Persistence.Views;

namespace NotificationLog.RentalService.Infrastructure.Persistence.ReadRepositories;

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

        return (items.Select(ToDto).ToList(), (int)stats.TotalResults);
    }

    public async Task<VisitDto?> GetAsync(Guid id, CancellationToken ct)
        => await _session.LoadAsync<VisitView>(id, ct) is { } view ? ToDto(view) : null;

    internal static VisitDto ToDto(VisitView x) => new(
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
        x.ListingType?.ToString(),
        x.ListingNeighborhood,
        x.ListingCity,
        x.VisitorName,
        x.HostName,
        x.History
            .Select(entry => new VisitHistoryEntryDto(entry.At, entry.By.ToString(), entry.Action, entry.Slots, entry.Reason, entry.IsLate))
            .ToList());
}
