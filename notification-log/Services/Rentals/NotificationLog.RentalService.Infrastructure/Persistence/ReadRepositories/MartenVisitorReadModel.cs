using Application.Shared.Pagination;
using Marten;
using Marten.Linq;
using NotificationLog.RentalService.Application.Visitors.Queries;
using NotificationLog.RentalService.Application.Visitors.Queries.Dtos;
using NotificationLog.RentalService.Application.Visitors.Queries.Filters;
using NotificationLog.RentalService.Domain.Visits.Enums;
using NotificationLog.RentalService.Infrastructure.Persistence.Views;

namespace NotificationLog.RentalService.Infrastructure.Persistence.ReadRepositories;

internal sealed class MartenVisitorReadModel : IVisitorReadModel
{
    private readonly IQuerySession _session;

    public MartenVisitorReadModel(IQuerySession session) => _session = session;

    public async Task<(IReadOnlyList<VisitorDto> Items, int TotalCount)> ListAsync(
        VisitorFilter filter, PageRequest paging, CancellationToken ct)
    {
        IQueryable<VisitorView> visitors = _session.Query<VisitorView>();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            visitors = visitors.Where(x =>
                x.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                || x.Email.Contains(search, StringComparison.OrdinalIgnoreCase)
                || x.Phone.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (filter.HasVisits is { } hasVisits)
            visitors = hasVisits ? visitors.Where(x => x.VisitCount > 0) : visitors.Where(x => x.VisitCount == 0);

        visitors = filter.Sort switch
        {
            VisitorSort.MostVisits => visitors.OrderByDescending(x => x.VisitCount).ThenByDescending(x => x.RegisteredAt),
            VisitorSort.Name => visitors.OrderBy(x => x.Name),
            _ => visitors.OrderByDescending(x => x.RegisteredAt)
        };

        var items = await ((IMartenQueryable<VisitorView>)visitors)
            .Stats(out QueryStatistics stats)
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .ToListAsync(ct);

        return (items.Select(ToDto).ToList(), (int)stats.TotalResults);
    }

    public async Task<VisitorDetailDto?> GetAsync(Guid id, CancellationToken ct)
    {
        if (await _session.LoadAsync<VisitorView>(id, ct) is not { } view)
            return null;

        return new VisitorDetailDto(
            ToDto(view),
            view.Visits.AsEnumerable().Reverse().Select(ToDto).ToList(),
            view.Activity.AsEnumerable().Reverse().Select(ToDto).ToList());
    }

    public async Task<VisitorsSummaryDto> GetSummaryAsync(DateTimeOffset registeredSince, CancellationToken ct)
    {
        var batch = _session.CreateBatchQuery();

        var total = batch.Query<VisitorView>().Count();
        var newLastWeek = batch.Query<VisitorView>().Where(x => x.RegisteredAt >= registeredSince).Count();
        var withoutVisits = batch.Query<VisitorView>().Where(x => x.VisitCount == 0).Count();

        await batch.Execute(ct);

        return new VisitorsSummaryDto((int)await total, (int)await newLastWeek, (int)await withoutVisits);
    }

    private static VisitorDto ToDto(VisitorView x) => new(
        x.Id,
        x.Name,
        x.Email,
        x.Phone,
        x.RegisteredAt,
        x.VisitCount,
        x.NoShowCount,
        NextStep(x.Visits));

    private static VisitorNextStepDto? NextStep(IEnumerable<VisitorVisitEntry> visits) => visits
        .Select(visit => visit.Status switch
        {
            VisitStatus.AwaitingVisitor => (Priority: 0, At: visit.RespondBy, Visit: visit),
            VisitStatus.Scheduled => (Priority: 1, At: visit.ScheduledStartsAt, Visit: visit),
            VisitStatus.AwaitingHost => (Priority: 2, At: (DateTimeOffset?)visit.UpdatedAt, Visit: visit),
            _ => (Priority: -1, At: null, Visit: visit)
        })
        .Where(step => step.Priority >= 0 && step.At is not null)
        .OrderBy(step => step.Priority)
        .ThenBy(step => step.At)
        .Select(step => new VisitorNextStepDto(
            step.Visit.VisitId, step.Visit.Status.ToString(), step.At!.Value, step.Visit.ListingNeighborhood))
        .FirstOrDefault();

    private static VisitorVisitDto ToDto(VisitorVisitEntry x) => new(
        x.VisitId,
        x.ListingId,
        x.ListingType?.ToString(),
        x.ListingNeighborhood,
        x.ListingCity,
        x.HostName,
        x.Status.ToString(),
        x.ScheduledStartsAt ?? (x.ProposedSlots.Count > 0 ? x.ProposedSlots[0] : null),
        x.RespondBy,
        x.RequestedAt,
        x.UpdatedAt);

    private static VisitorActivityDto ToDto(VisitorActivityEntry x) => new(
        x.At,
        x.Action,
        x.By?.ToString(),
        x.VisitId,
        x.ListingNeighborhood,
        x.HostName,
        x.Slot,
        x.IsLate);
}
