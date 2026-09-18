using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.Application.Listings.Commands.UpdateListingDetails;

public sealed record UpdateListingDetailsCommand(
    Guid ListingId,
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
    string Description);
