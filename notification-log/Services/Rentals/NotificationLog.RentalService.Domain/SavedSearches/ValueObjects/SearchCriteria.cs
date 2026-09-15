using Domain.Shared.Exceptions;
using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Domain.SavedSearches.ValueObjects;

public sealed record SearchCriteria
{
    public const int MaxCityLength = 100;
    public const int MaxNeighborhoods = 20;
    public const int MaxNeighborhoodLength = 100;

    public Operation Operation { get; }
    public string City { get; }
    public IReadOnlyList<string> Neighborhoods { get; }
    public PriceRange? Price { get; }
    public int? MinBedrooms { get; }
    public Stratum? MinStratum { get; }
    public Stratum? MaxStratum { get; }
    public decimal? MinArea { get; }

    private SearchCriteria(
        Operation operation,
        string city,
        IReadOnlyList<string> neighborhoods,
        PriceRange? price,
        int? minBedrooms,
        Stratum? minStratum,
        Stratum? maxStratum,
        decimal? minArea)
    {
        Operation = operation;
        City = city;
        Neighborhoods = neighborhoods;
        Price = price;
        MinBedrooms = minBedrooms;
        MinStratum = minStratum;
        MaxStratum = maxStratum;
        MinArea = minArea;
    }

    public static SearchCriteria Create(
        Operation operation,
        string city,
        IReadOnlyList<string> neighborhoods,
        PriceRange? price,
        int? minBedrooms,
        Stratum? minStratum,
        Stratum? maxStratum,
        decimal? minArea)
    {
        ArgumentNullException.ThrowIfNull(neighborhoods);

        if (!Enum.IsDefined(operation))
            throw new DomainException("La operación de la búsqueda no es válida.");

        if (string.IsNullOrWhiteSpace(city))
            throw new DomainException("La ciudad de la búsqueda es obligatoria.");

        var normalizedCity = city.Trim();

        if (normalizedCity.Length > MaxCityLength)
            throw new DomainException($"La ciudad no puede superar {MaxCityLength} caracteres.");

        var normalizedNeighborhoods = neighborhoods
            .Where(neighborhood => !string.IsNullOrWhiteSpace(neighborhood))
            .Select(neighborhood => neighborhood.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();

        if (normalizedNeighborhoods.Count > MaxNeighborhoods)
            throw new DomainException($"Una búsqueda admite como máximo {MaxNeighborhoods} barrios.");

        if (normalizedNeighborhoods.Any(neighborhood => neighborhood.Length > MaxNeighborhoodLength))
            throw new DomainException($"Un barrio no puede superar {MaxNeighborhoodLength} caracteres.");

        if (minBedrooms < 0)
            throw new DomainException("El mínimo de habitaciones no puede ser negativo.");

        if (minStratum is not null && maxStratum is not null && minStratum.Value > maxStratum.Value)
            throw new DomainException("El estrato mínimo no puede ser mayor que el máximo.");

        if (minArea <= 0)
            throw new DomainException("El área mínima debe ser mayor que cero.");

        return new SearchCriteria(
            operation, normalizedCity, normalizedNeighborhoods, price, minBedrooms, minStratum, maxStratum, minArea);
    }

    internal static SearchCriteria FromStorage(
        Operation operation,
        string city,
        IReadOnlyList<string> neighborhoods,
        PriceRange? price,
        int? minBedrooms,
        Stratum? minStratum,
        Stratum? maxStratum,
        decimal? minArea)
        => new(operation, city, neighborhoods, price, minBedrooms, minStratum, maxStratum, minArea);

    public bool Equals(SearchCriteria? other)
        => other is not null
            && Operation == other.Operation
            && City == other.City
            && Neighborhoods.SequenceEqual(other.Neighborhoods)
            && Price == other.Price
            && MinBedrooms == other.MinBedrooms
            && MinStratum == other.MinStratum
            && MaxStratum == other.MaxStratum
            && MinArea == other.MinArea;

    public override int GetHashCode()
        => HashCode.Combine(Operation, City, Neighborhoods.Count, Price, MinBedrooms, MinStratum, MaxStratum, MinArea);
}
