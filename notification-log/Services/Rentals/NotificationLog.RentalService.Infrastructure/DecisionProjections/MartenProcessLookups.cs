using Marten;
using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Domain.Listings.Enums;
using NotificationLog.RentalService.Domain.SavedSearches;
using NotificationLog.RentalService.Domain.Visits.Enums;

namespace NotificationLog.RentalService.Infrastructure.DecisionProjections;

internal sealed class MartenProcessLookups : IProcessLookups
{
    private readonly IDocumentSession _session;

    public MartenProcessLookups(IDocumentSession session) => _session = session;

    public async Task<IReadOnlyList<Guid>> FindOpenOfferIdsAsync(Guid listingId, CancellationToken ct)
        => await _session.Query<OfferDecision>()
            .Where(x => x.ListingId == listingId && x.IsOpen)
            .Select(x => x.Id)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Guid>> FindUpcomingVisitIdsAsync(Guid listingId, CancellationToken ct)
        => await _session.Query<VisitDecision>()
            .Where(x => x.ListingId == listingId
                && (x.Status == VisitStatus.Requested || x.Status == VisitStatus.Confirmed))
            .Select(x => x.Id)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Guid>> FindOpenInquiryIdsAsync(Guid listingId, CancellationToken ct)
        => await _session.Query<InquiryDecision>()
            .Where(x => x.ListingId == listingId && !x.IsClosed)
            .Select(x => x.Id)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Guid>> FindFavoritedByAsync(Guid listingId, CancellationToken ct)
        => await _session.Query<FavoriteListDecision>()
            .Where(x => x.ListingIds.Contains(listingId))
            .Select(x => x.UserId)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<SavedSearchMatch>> FindImmediateMatchesAsync(Guid listingId, CancellationToken ct)
    {
        var listing = await _session.LoadAsync<ListingDecision>(listingId, ct);

        if (listing is not { Status: ListingStatus.Published })
            return [];

        var lists = await ListsWithFrequencyAsync(AlertFrequency.Immediate, ct);

        return lists
            .SelectMany(list => list.Searches
                .Where(search => search.Frequency == AlertFrequency.Immediate && search.Matches(listing))
                .Select(search => new SavedSearchMatch(list.UserId, search.SearchId, search.Name)))
            .ToList();
    }

    public async Task<IReadOnlyList<SavedSearchDigest>> FindDailyDigestsAsync(DateTimeOffset since, CancellationToken ct)
    {
        var listings = await _session.Query<ListingDecision>()
            .Where(x => x.Status == ListingStatus.Published && x.PublishedAt >= since)
            .ToListAsync(ct);

        if (listings.Count == 0)
            return [];

        var lists = await ListsWithFrequencyAsync(AlertFrequency.Daily, ct);

        return lists
            .SelectMany(list => list.Searches
                .Where(search => search.Frequency == AlertFrequency.Daily)
                .Select(search => new SavedSearchDigest(
                    list.UserId,
                    search.Name,
                    listings.Where(search.Matches).Select(listing => listing.Id).ToList())))
            .Where(digest => digest.ListingIds.Count > 0)
            .ToList();
    }

    private async Task<IReadOnlyList<SavedSearchListDecision>> ListsWithFrequencyAsync(AlertFrequency frequency, CancellationToken ct)
        => await _session.Query<SavedSearchListDecision>()
            .Where(x => x.Searches.Any(search => search.Frequency == frequency))
            .ToListAsync(ct);
}
