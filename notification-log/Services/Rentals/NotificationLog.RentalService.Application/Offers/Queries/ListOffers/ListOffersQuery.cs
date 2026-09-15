using NotificationLog.RentalService.Domain.Offers.Enums;

namespace NotificationLog.RentalService.Application.Offers.Queries.ListOffers;

public sealed record ListOffersQuery(
    Guid? ListingId = null,
    Guid? OffererId = null,
    OfferStatus? Status = null,
    int Page = 1,
    int PageSize = 20);
