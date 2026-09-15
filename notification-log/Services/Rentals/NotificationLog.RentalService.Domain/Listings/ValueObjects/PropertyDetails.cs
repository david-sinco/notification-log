using Domain.Shared.Exceptions;
using NotificationLog.RentalService.Domain.Shared;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.Domain.Listings.ValueObjects;

public sealed record PropertyDetails
{
    public PropertyType Type { get; }
    public decimal Area { get; }
    public int Bedrooms { get; }
    public int Bathrooms { get; }
    public int ParkingSpots { get; }
    public Stratum Stratum { get; }
    public int? Floor { get; }
    public bool HasElevator { get; }
    public Money AdministrationFee { get; }

    private PropertyDetails(
        PropertyType type,
        decimal area,
        int bedrooms,
        int bathrooms,
        int parkingSpots,
        Stratum stratum,
        int? floor,
        bool hasElevator,
        Money administrationFee)
    {
        Type = type;
        Area = area;
        Bedrooms = bedrooms;
        Bathrooms = bathrooms;
        ParkingSpots = parkingSpots;
        Stratum = stratum;
        Floor = floor;
        HasElevator = hasElevator;
        AdministrationFee = administrationFee;
    }

    public static PropertyDetails Create(
        PropertyType type,
        decimal area,
        int bedrooms,
        int bathrooms,
        int parkingSpots,
        Stratum stratum,
        int? floor,
        bool hasElevator,
        Money administrationFee)
    {
        ArgumentNullException.ThrowIfNull(stratum);
        ArgumentNullException.ThrowIfNull(administrationFee);

        if (!Enum.IsDefined(type))
            throw new DomainException("El tipo de inmueble no es válido.");

        if (area < ListingPolicy.MinArea)
            throw new DomainException($"El área debe ser de al menos {ListingPolicy.MinArea} m².");

        if (bedrooms < 0)
            throw new DomainException("El número de habitaciones no puede ser negativo.");

        if (bathrooms < 1)
            throw new DomainException("El inmueble debe tener al menos un baño.");

        if (parkingSpots < 0)
            throw new DomainException("El número de parqueaderos no puede ser negativo.");

        if (floor < 1)
            throw new DomainException("El piso debe ser 1 o mayor.");

        return new PropertyDetails(
            type, area, bedrooms, bathrooms, parkingSpots, stratum, floor, hasElevator, administrationFee);
    }

    internal static PropertyDetails FromStorage(
        PropertyType type,
        decimal area,
        int bedrooms,
        int bathrooms,
        int parkingSpots,
        Stratum stratum,
        int? floor,
        bool hasElevator,
        Money administrationFee)
        => new(type, area, bedrooms, bathrooms, parkingSpots, stratum, floor, hasElevator, administrationFee);
}
