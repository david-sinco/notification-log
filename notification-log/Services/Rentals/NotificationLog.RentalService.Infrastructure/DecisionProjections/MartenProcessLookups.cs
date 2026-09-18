using Marten;
using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Domain.Visits.Enums;

namespace NotificationLog.RentalService.Infrastructure.DecisionProjections;

internal sealed class MartenProcessLookups : IProcessLookups
{
    private readonly IDocumentSession _session;

    public MartenProcessLookups(IDocumentSession session) => _session = session;

    public async Task<IReadOnlyList<Guid>> FindUpcomingVisitIdsAsync(Guid listingId, CancellationToken ct)
        => await _session.Query<VisitDecision>()
            .Where(x => x.ListingId == listingId
                && (x.Status == VisitStatus.Requested || x.Status == VisitStatus.Confirmed))
            .Select(x => x.Id)
            .ToListAsync(ct);
}
