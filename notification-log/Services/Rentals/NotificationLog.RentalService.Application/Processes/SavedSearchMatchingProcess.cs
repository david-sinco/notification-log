using NotificationLog.RentalService.Application.Abstractions;

namespace NotificationLog.RentalService.Application.Processes;

public sealed class SavedSearchMatchingProcess
{
    private readonly IProcessLookups _lookups;
    private readonly IIdentityReplica _identity;
    private readonly INotificationDispatcher _notifications;

    public SavedSearchMatchingProcess(IProcessLookups lookups, IIdentityReplica identity, INotificationDispatcher notifications)
        => (_lookups, _identity, _notifications) = (lookups, identity, notifications);

    public async Task NotifyImmediateMatchesAsync(Guid listingId, CancellationToken ct)
    {
        foreach (var match in await _lookups.FindImmediateMatchesAsync(listingId, ct))
        {
            if (!await _identity.HasAlertsConsentAsync(match.UserId, ct))
                continue;

            await _notifications.DispatchAsync(
                NotificationKeys.SearchMatch,
                match.UserId,
                new Dictionary<string, string>
                {
                    ["listing_id"] = listingId.ToString(),
                    ["search_name"] = match.SearchName
                },
                ct);
        }
    }

    public async Task SendDailyDigestAsync(DateTimeOffset since, CancellationToken ct)
    {
        foreach (var digest in await _lookups.FindDailyDigestsAsync(since, ct))
        {
            if (digest.ListingIds.Count == 0 || !await _identity.HasAlertsConsentAsync(digest.UserId, ct))
                continue;

            await _notifications.DispatchAsync(
                NotificationKeys.SearchDigest,
                digest.UserId,
                new Dictionary<string, string>
                {
                    ["search_name"] = digest.SearchName,
                    ["listing_count"] = digest.ListingIds.Count.ToString(),
                    ["listing_ids"] = string.Join(",", digest.ListingIds)
                },
                ct);
        }
    }
}
