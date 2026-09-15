using JasperFx.Events;
using Marten.Events.Aggregation;
using NotificationLog.RentalService.Domain.SavedSearches.Events;

namespace NotificationLog.RentalService.Infrastructure.DecisionProjections;

public sealed class SavedSearchListDecisionProjection : SingleStreamProjection<SavedSearchListDecision, Guid>
{
    public override SavedSearchListDecision? Evolve(SavedSearchListDecision? snapshot, Guid id, IEvent e)
    {
        if (e.Data is SavedSearchListStarted started)
            return new SavedSearchListDecision { Id = id, UserId = started.UserId };

        if (snapshot is null)
            return null;

        switch (e.Data)
        {
            case SavedSearchCreated c:
                snapshot.Searches.Add(new SavedSearchEntry { SearchId = c.SearchId });
                Fill(snapshot.Searches[^1], c.Name, c);
                break;
            case SavedSearchUpdated u when snapshot.Searches.Find(s => s.SearchId == u.SearchId) is { } entry:
                Fill(entry, u.Name, u);
                break;
            case SavedSearchDeleted d:
                snapshot.Searches.RemoveAll(s => s.SearchId == d.SearchId);
                break;
        }

        return snapshot;
    }

    private static void Fill(SavedSearchEntry entry, string name, SavedSearchCreated e)
        => Fill(entry, name, e.Operation, e.City, e.Neighborhoods, e.MinPrice, e.MaxPrice, e.MinBedrooms, e.MinStratum, e.MaxStratum, e.MinArea, e.Frequency);

    private static void Fill(SavedSearchEntry entry, string name, SavedSearchUpdated e)
        => Fill(entry, name, e.Operation, e.City, e.Neighborhoods, e.MinPrice, e.MaxPrice, e.MinBedrooms, e.MinStratum, e.MaxStratum, e.MinArea, e.Frequency);

    private static void Fill(
        SavedSearchEntry entry,
        string name,
        Domain.Shared.Operation operation,
        string city,
        IReadOnlyList<string> neighborhoods,
        long? minPrice,
        long? maxPrice,
        int? minBedrooms,
        int? minStratum,
        int? maxStratum,
        decimal? minArea,
        Domain.SavedSearches.AlertFrequency frequency)
    {
        entry.Name = name;
        entry.Operation = operation;
        entry.City = city;
        entry.Neighborhoods = [.. neighborhoods];
        entry.MinPrice = minPrice;
        entry.MaxPrice = maxPrice;
        entry.MinBedrooms = minBedrooms;
        entry.MinStratum = minStratum;
        entry.MaxStratum = maxStratum;
        entry.MinArea = minArea;
        entry.Frequency = frequency;
    }
}
