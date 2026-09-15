using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Api.Contracts.SavedSearches;

public sealed record SearchCriteriaRequest(
    Operation Operation,
    string City,
    IReadOnlyList<string> Neighborhoods,
    long? MinPrice,
    long? MaxPrice,
    int? MinBedrooms,
    int? MinStratum,
    int? MaxStratum,
    decimal? MinArea
);
