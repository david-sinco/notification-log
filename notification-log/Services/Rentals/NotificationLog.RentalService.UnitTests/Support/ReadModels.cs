using NotificationLog.RentalService.Application.Dashboard.Queries.Dtos;
using NotificationLog.RentalService.Application.Listings.Queries.Dtos;
using NotificationLog.RentalService.Application.Owners.Queries.Dtos;
using NotificationLog.RentalService.Application.Visitors.Queries.Dtos;
using NotificationLog.RentalService.Application.Visits.Queries.Dtos;

namespace NotificationLog.RentalService.UnitTests.Support;

public static class ReadModels
{
    public static ListingDto Listing(Guid id)
        => new(
            id, ListingFactory.Owner, ListingFactory.Owner, ListingFactory.Owner, "Rent", "Published", "Apartment", 68m, 2, 2, 1, 4, 5, true,
            320_000, "Bogotá", "Chapinero", "Calle 60 # 9-45", "Descripción", 1_800_000, [], null, [], null, null, null,
            Clock.Now, Clock.Now, "Ana Gómez Rincón");

    public static ListingSummaryDto ListingSummary()
        => new(
            Guid.NewGuid(), "Rent", "Published", "Apartment", "Bogotá", "Chapinero", 1_800_000, 2, 68m,
            ListingFactory.Owner, ListingFactory.Owner, Clock.Now, "Ana Gómez Rincón", null, 5);

    public static DashboardDto Dashboard()
        => new(0, null, 0, null, 0, 0, null, null, [ListingSummary()], 0, []);

    public static OwnerDto Owner(Guid id, Guid createdBy, Guid? relatedUserId = null)
        => new(
            id, createdBy, relatedUserId, "Ana Gómez Rincón", "ana@example.com", "+573001234567", Clock.Now, 0);

    public static VisitorDto Visitor(Guid id)
        => new(id, "Víctor Rojas Peña", "victor@example.com", "+573109876543", Clock.Now, 0, 0, null);

    public static VisitorDetailDto VisitorDetail(Guid id)
        => new(Visitor(id), [], []);

    public static VisitDto Visit(Guid id)
        => new(
            id, Guid.NewGuid(), VisitFactory.Host, VisitFactory.Visitor, "AwaitingHost", [], null, null, null, null, null,
            false, null, Clock.Now, Clock.Now, "Apartment", "Chapinero", "Bogotá", "Víctor Rojas Peña", "Ana Gómez Rincón", []);
}
