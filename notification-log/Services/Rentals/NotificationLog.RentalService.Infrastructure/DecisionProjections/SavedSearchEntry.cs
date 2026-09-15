using NotificationLog.RentalService.Domain.SavedSearches;
using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Infrastructure.DecisionProjections;

public sealed class SavedSearchEntry
{
    public Guid SearchId { get; set; }
    public string Name { get; set; } = string.Empty;
    public Operation Operation { get; set; }
    public string City { get; set; } = string.Empty;
    public List<string> Neighborhoods { get; set; } = [];
    public long? MinPrice { get; set; }
    public long? MaxPrice { get; set; }
    public int? MinBedrooms { get; set; }
    public int? MinStratum { get; set; }
    public int? MaxStratum { get; set; }
    public decimal? MinArea { get; set; }
    public AlertFrequency Frequency { get; set; }

    public bool Matches(ListingDecision listing)
        => Operation == listing.Operation
            && string.Equals(City, listing.City, StringComparison.OrdinalIgnoreCase)
            && (Neighborhoods.Count == 0 || Neighborhoods.Contains(listing.Neighborhood, StringComparer.OrdinalIgnoreCase))
            && (MinPrice is null || listing.Price >= MinPrice)
            && (MaxPrice is null || listing.Price <= MaxPrice)
            && (MinBedrooms is null || listing.Bedrooms >= MinBedrooms)
            && (MinStratum is null || listing.Stratum >= MinStratum)
            && (MaxStratum is null || listing.Stratum <= MaxStratum)
            && (MinArea is null || listing.Area >= MinArea);
}
