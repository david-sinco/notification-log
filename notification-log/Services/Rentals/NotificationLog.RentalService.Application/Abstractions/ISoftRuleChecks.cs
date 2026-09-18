using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Application.Abstractions;

public interface ISoftRuleChecks
{
    Task<int> CountPendingVisitsAsync(Guid visitorId, CancellationToken ct);

    Task<IReadOnlyList<DateTimeOffset>> GetVisitStrikesAsync(Guid visitorId, DateTimeOffset since, CancellationToken ct);

    Task<bool> HostHasOverlapAsync(Guid hostId, TimeSlot slot, CancellationToken ct);
}
