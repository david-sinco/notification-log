using Marten;
using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Domain.Shared;
using NotificationLog.RentalService.Domain.Visits.Enums;

namespace NotificationLog.RentalService.Infrastructure.DecisionProjections;

internal sealed class MartenSoftRuleChecks : ISoftRuleChecks
{
    private readonly IDocumentSession _session;

    public MartenSoftRuleChecks(IDocumentSession session) => _session = session;

    public Task<int> CountPendingVisitsAsync(Guid visitorId, CancellationToken ct)
        => _session.Query<VisitDecision>()
            .CountAsync(x => x.VisitorId == visitorId
                && (x.Status == VisitStatus.Requested || x.Status == VisitStatus.Confirmed), ct);

    public async Task<IReadOnlyList<DateTimeOffset>> GetVisitStrikesAsync(Guid visitorId, DateTimeOffset since, CancellationToken ct)
    {
        var visits = await _session.Query<VisitDecision>()
            .Where(x => x.VisitorId == visitorId && x.StrikeAt != null)
            .ToListAsync(ct);

        return visits.Select(x => x.StrikeAt!.Value).Where(at => at >= since).ToList();
    }

    public Task<bool> HostHasOverlapAsync(Guid hostId, TimeSlot slot, CancellationToken ct)
        => _session.Query<VisitDecision>()
            .AnyAsync(x => x.HostId == hostId
                && x.Status == VisitStatus.Confirmed
                && x.SlotStart < slot.End
                && x.SlotEnd > slot.Start, ct);
}
