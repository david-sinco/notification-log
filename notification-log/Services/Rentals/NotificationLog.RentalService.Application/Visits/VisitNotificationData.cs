using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;
using NotificationLog.RentalService.Domain.Visits;

namespace NotificationLog.RentalService.Application.Visits;

public static class VisitNotificationData
{
    public static Dictionary<string, string> For(Visit visit, Listing listing) => new()
    {
        ["visit_id"] = visit.Id.ToString(),
        ["listing_id"] = listing.Id.ToString(),
        ["listing_type"] = listing.Details!.Type == PropertyType.Studio ? "Apartaestudio" : "Apartamento",
        ["listing_operation"] = listing.Operation == Operation.Rent ? "arriendo" : "venta",
        ["listing_neighborhood"] = listing.Location!.Neighborhood,
        ["listing_city"] = listing.Location.City,
        ["listing_address"] = listing.Location.Address,
        ["listing_price"] = listing.Price!.ToString()
    };
}
