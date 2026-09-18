namespace NotificationLog.Web.Api.Rentals.Users;

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

public sealed record SearchCriteriaRequest(
    Operation Operation,
    string City,
    IReadOnlyList<string> Neighborhoods,
    long? MinPrice,
    long? MaxPrice,
    int? MinBedrooms,
    int? MinStratum,
    int? MaxStratum,
    decimal? MinArea);

public sealed record SavedSearchRequest(string Name, SearchCriteriaRequest Criteria, AlertFrequency Frequency);

public static class SavedSearchLimits
{
    public const int MaxSearches = 10;
    public const int MaxNameLength = 80;
}
