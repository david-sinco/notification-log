using Marten;
using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Domain.Listings.Enums;
using NotificationLog.RentalService.Domain.Shared;
using NotificationLog.RentalService.Domain.Visits.Enums;

namespace NotificationLog.RentalService.Infrastructure.DecisionProjections;

internal sealed class MartenSoftRuleChecks : ISoftRuleChecks
{
    private readonly IDocumentSession _session;

    public MartenSoftRuleChecks(IDocumentSession session) => _session = session;

    public Task<int> CountActiveListingsAsync(Guid publisherId, CancellationToken ct)
        => _session.Query<ListingDecision>()
            .CountAsync(x => x.PublisherId == publisherId
                && (x.Status == ListingStatus.InReview
                    || x.Status == ListingStatus.Published
                    || x.Status == ListingStatus.Paused
                    || x.Status == ListingStatus.Reserved), ct);

    public Task<int> CountActiveListingsByAdvisorAsync(Guid advisorId, CancellationToken ct)
        => _session.Query<ListingDecision>()
            .CountAsync(x => x.AdvisorId == advisorId
                && (x.Status == ListingStatus.InReview
                    || x.Status == ListingStatus.Published
                    || x.Status == ListingStatus.Paused
                    || x.Status == ListingStatus.Reserved), ct);

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

    public Task<int> CountInquiriesOpenedSinceAsync(Guid seekerId, DateTimeOffset since, CancellationToken ct)
        => _session.Query<InquiryDecision>()
            .CountAsync(x => x.SeekerId == seekerId && x.OpenedAt >= since, ct);

    public Task<bool> HasCompletedVisitAsync(Guid visitorId, Guid listingId, CancellationToken ct)
        => _session.Query<VisitDecision>()
            .AnyAsync(x => x.VisitorId == visitorId && x.ListingId == listingId && x.Status == VisitStatus.Completed, ct);

    public Task<bool> HasOpenOfferAsync(Guid offererId, Guid listingId, CancellationToken ct)
        => _session.Query<OfferDecision>()
            .AnyAsync(x => x.OffererId == offererId && x.ListingId == listingId && x.IsOpen, ct);
}
