namespace NotificationLog.RentalService.Application.Abstractions;

public interface IProcessLookups
{
    Task<IReadOnlyList<Guid>> FindUpcomingVisitIdsAsync(Guid listingId, CancellationToken ct);
}
