using Domain.Shared.Exceptions;

namespace NotificationLog.RentalService.Domain.Listings.ValueObjects;

public sealed record Location
{
    public const int MaxCityLength = 100;
    public const int MaxNeighborhoodLength = 100;
    public const int MaxAddressLength = 200;

    public string City { get; }
    public string Neighborhood { get; }
    public string Address { get; }

    private Location(string city, string neighborhood, string address)
    {
        City = city;
        Neighborhood = neighborhood;
        Address = address;
    }

    public static Location Create(string city, string neighborhood, string address)
    {
        if (string.IsNullOrWhiteSpace(city))
            throw new DomainException("La ciudad es obligatoria.");

        if (string.IsNullOrWhiteSpace(neighborhood))
            throw new DomainException("El barrio es obligatorio.");

        if (string.IsNullOrWhiteSpace(address))
            throw new DomainException("La dirección es obligatoria.");

        var normalizedCity = city.Trim();
        var normalizedNeighborhood = neighborhood.Trim();
        var normalizedAddress = address.Trim();

        if (normalizedCity.Length > MaxCityLength)
            throw new DomainException($"La ciudad no puede superar {MaxCityLength} caracteres.");

        if (normalizedNeighborhood.Length > MaxNeighborhoodLength)
            throw new DomainException($"El barrio no puede superar {MaxNeighborhoodLength} caracteres.");

        if (normalizedAddress.Length > MaxAddressLength)
            throw new DomainException($"La dirección no puede superar {MaxAddressLength} caracteres.");

        return new Location(normalizedCity, normalizedNeighborhood, normalizedAddress);
    }

    internal static Location FromStorage(string city, string neighborhood, string address)
        => new(city, neighborhood, address);
}
