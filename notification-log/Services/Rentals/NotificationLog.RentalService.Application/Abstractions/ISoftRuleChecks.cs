using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Application.Abstractions;

public interface ISoftRuleChecks
{
    Task<int> CountActiveListingsAsync(Guid publisherId, CancellationToken ct);

    Task<int> CountActiveListingsByAdvisorAsync(Guid advisorId, CancellationToken ct);

    Task<int> CountPendingVisitsAsync(Guid visitorId, CancellationToken ct);

    Task<IReadOnlyList<DateTimeOffset>> GetVisitStrikesAsync(Guid visitorId, DateTimeOffset since, CancellationToken ct);

    Task<bool> HostHasOverlapAsync(Guid hostId, TimeSlot slot, CancellationToken ct);

    Task<int> CountInquiriesOpenedSinceAsync(Guid seekerId, DateTimeOffset since, CancellationToken ct);

    Task<bool> HasCompletedVisitAsync(Guid visitorId, Guid listingId, CancellationToken ct);

    Task<bool> HasOpenOfferAsync(Guid offererId, Guid listingId, CancellationToken ct);
}
