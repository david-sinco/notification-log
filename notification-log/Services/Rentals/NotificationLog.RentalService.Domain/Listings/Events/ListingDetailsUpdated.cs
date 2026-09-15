using Domain.Shared.EventSourcing;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.Domain.Listings.Events;

public sealed record ListingDetailsUpdated(
    PropertyType Type,
    decimal Area,
    int Bedrooms,
    int Bathrooms,
    int ParkingSpots,
    int Stratum,
    int? Floor,
    bool HasElevator,
    long AdministrationFee,
    string City,
    string Neighborhood,
    string Address,
    string Description
) : DomainEvent;
