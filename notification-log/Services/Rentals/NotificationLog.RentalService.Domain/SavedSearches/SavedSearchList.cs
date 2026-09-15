using Domain.Shared.Common;
using Domain.Shared.EventSourcing;
using Domain.Shared.Exceptions;
using NotificationLog.RentalService.Domain.SavedSearches.Events;
using NotificationLog.RentalService.Domain.SavedSearches.ValueObjects;
using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Domain.SavedSearches;

public sealed class SavedSearchList : AggregateRoot
{
    public const int MaxSearches = 10;
    public const int MaxNameLength = 80;

    private readonly List<SavedSearch> _searches = [];

    private SavedSearchList() { }

    public Guid UserId { get; private set; }
    public IReadOnlyList<SavedSearch> Searches => _searches.AsReadOnly();

    public static SavedSearchList StartFor(Guid userId)
    {
        Guard.RequireId(userId, "El usuario es obligatorio.");

        var searches = new SavedSearchList();
        searches.Raise(new SavedSearchListStarted(SavedSearchListId.For(userId), userId));

        return searches;
    }

    public void Add(Guid searchId, string name, SearchCriteria criteria, AlertFrequency frequency)
    {
        Guard.RequireId(searchId, "El identificador de la búsqueda es obligatorio.");
        ArgumentNullException.ThrowIfNull(criteria);
        EnsureFrequency(frequency);
        var normalizedName = NormalizeName(name);

        if (_searches.Any(search => search.Id == searchId))
            throw new DomainException("Ya existe una búsqueda guardada con ese identificador.");

        if (_searches.Count >= MaxSearches)
            throw new DomainException($"No se pueden tener más de {MaxSearches} búsquedas guardadas.");

        Raise(new SavedSearchCreated(
            searchId,
            normalizedName,
            criteria.Operation,
            criteria.City,
            criteria.Neighborhoods,
            criteria.Price?.Min?.Amount,
            criteria.Price?.Max?.Amount,
            criteria.MinBedrooms,
            criteria.MinStratum?.Value,
            criteria.MaxStratum?.Value,
            criteria.MinArea,
            frequency));
    }

    public void Update(Guid searchId, string name, SearchCriteria criteria, AlertFrequency frequency)
    {
        ArgumentNullException.ThrowIfNull(criteria);
        EnsureFrequency(frequency);
        var normalizedName = NormalizeName(name);

        var search = _searches.FirstOrDefault(candidate => candidate.Id == searchId)
            ?? throw new DomainException("La búsqueda guardada no existe.");

        if (search.Name == normalizedName && search.Criteria == criteria && search.Frequency == frequency)
            return;

        Raise(new SavedSearchUpdated(
            searchId,
            normalizedName,
            criteria.Operation,
            criteria.City,
            criteria.Neighborhoods,
            criteria.Price?.Min?.Amount,
            criteria.Price?.Max?.Amount,
            criteria.MinBedrooms,
            criteria.MinStratum?.Value,
            criteria.MaxStratum?.Value,
            criteria.MinArea,
            frequency));
    }

    public void Remove(Guid searchId)
    {
        if (_searches.All(search => search.Id != searchId))
            return;

        Raise(new SavedSearchDeleted(searchId));
    }

    public override void Apply(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case SavedSearchListStarted e:
                Id = e.SavedSearchListId;
                UserId = e.UserId;
                break;
            case SavedSearchCreated e: When(e); break;
            case SavedSearchUpdated e: When(e); break;
            case SavedSearchDeleted e: _searches.RemoveAll(search => search.Id == e.SearchId); break;

            default:
                throw new DomainException(
                    $"El evento '{domainEvent.GetType().Name}' no pertenece al agregado SavedSearchList.");
        }
    }

    private void When(SavedSearchCreated e)
        => _searches.Add(new SavedSearch(
            e.SearchId,
            e.Name,
            CriteriaFrom(e.Operation, e.City, e.Neighborhoods, e.MinPrice, e.MaxPrice, e.MinBedrooms, e.MinStratum, e.MaxStratum, e.MinArea),
            e.Frequency));

    private void When(SavedSearchUpdated e)
        => _searches.FirstOrDefault(search => search.Id == e.SearchId)?.Change(
            e.Name,
            CriteriaFrom(e.Operation, e.City, e.Neighborhoods, e.MinPrice, e.MaxPrice, e.MinBedrooms, e.MinStratum, e.MaxStratum, e.MinArea),
            e.Frequency);

    private static SearchCriteria CriteriaFrom(
        Operation operation,
        string city,
        IReadOnlyList<string> neighborhoods,
        long? minPrice,
        long? maxPrice,
        int? minBedrooms,
        int? minStratum,
        int? maxStratum,
        decimal? minArea)
        => SearchCriteria.FromStorage(
            operation,
            city,
            neighborhoods,
            minPrice is null && maxPrice is null ? null : PriceRange.FromStorage(ToMoney(minPrice), ToMoney(maxPrice)),
            minBedrooms,
            minStratum is { } min ? Stratum.FromStorage(min) : null,
            maxStratum is { } max ? Stratum.FromStorage(max) : null,
            minArea);

    private static Money? ToMoney(long? amount) => amount is { } value ? Money.FromStorage(value) : null;

    private static void EnsureFrequency(AlertFrequency frequency)
    {
        if (!Enum.IsDefined(frequency))
            throw new DomainException("La frecuencia de alertas no es válida.");
    }

    private static string NormalizeName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("El nombre de la búsqueda es obligatorio.");

        var normalized = name.Trim();

        if (normalized.Length > MaxNameLength)
            throw new DomainException($"El nombre de la búsqueda no puede superar {MaxNameLength} caracteres.");

        return normalized;
    }
}
