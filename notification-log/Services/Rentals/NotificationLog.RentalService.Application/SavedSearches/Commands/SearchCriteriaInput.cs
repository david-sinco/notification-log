using NotificationLog.RentalService.Domain.SavedSearches.ValueObjects;
using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Application.SavedSearches.Commands;

public sealed record SearchCriteriaInput(
    Operation Operation,
    string City,
    IReadOnlyList<string> Neighborhoods,
    long? MinPrice,
    long? MaxPrice,
    int? MinBedrooms,
    int? MinStratum,
    int? MaxStratum,
    decimal? MinArea)
{
    public SearchCriteria ToDomain() => SearchCriteria.Create(
        Operation,
        City,
        Neighborhoods,
        MinPrice is null && MaxPrice is null ? null : PriceRange.Create(ToMoney(MinPrice), ToMoney(MaxPrice)),
        MinBedrooms,
        MinStratum is { } min ? Stratum.Create(min) : null,
        MaxStratum is { } max ? Stratum.Create(max) : null,
        MinArea);

    private static Money? ToMoney(long? amount) => amount is { } value ? Money.Create(value) : null;
}
