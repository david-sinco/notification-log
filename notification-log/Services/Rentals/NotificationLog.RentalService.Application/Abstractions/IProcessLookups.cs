namespace NotificationLog.RentalService.Application.Abstractions;

public interface IProcessLookups
{
    Task<IReadOnlyList<Guid>> FindOpenOfferIdsAsync(Guid listingId, CancellationToken ct);

    Task<IReadOnlyList<Guid>> FindUpcomingVisitIdsAsync(Guid listingId, CancellationToken ct);

    Task<IReadOnlyList<Guid>> FindOpenInquiryIdsAsync(Guid listingId, CancellationToken ct);

    Task<IReadOnlyList<Guid>> FindFavoritedByAsync(Guid listingId, CancellationToken ct);

    Task<IReadOnlyList<SavedSearchMatch>> FindImmediateMatchesAsync(Guid listingId, CancellationToken ct);

    Task<IReadOnlyList<SavedSearchDigest>> FindDailyDigestsAsync(DateTimeOffset since, CancellationToken ct);
}
