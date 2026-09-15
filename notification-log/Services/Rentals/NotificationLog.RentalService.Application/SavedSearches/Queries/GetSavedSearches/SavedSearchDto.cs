namespace NotificationLog.RentalService.Application.SavedSearches.Queries.GetSavedSearches;

public sealed record SavedSearchDto(
    Guid Id,
    string Name,
    string Frequency,
    string Operation,
    string City,
    IReadOnlyList<string> Neighborhoods,
    long? MinPrice,
    long? MaxPrice,
    int? MinBedrooms,
    int? MinStratum,
    int? MaxStratum,
    decimal? MinArea);
