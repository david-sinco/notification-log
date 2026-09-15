using NotificationLog.RentalService.Application.Offers.Dtos;
using NotificationLog.RentalService.Application.Offers.Queries.ListOffers;

namespace NotificationLog.RentalService.Application.Abstractions;

public interface IOfferReadModel
{
    Task<(IReadOnlyList<OfferSummaryDto> Items, int TotalCount)> ListAsync(ListOffersQuery query, CancellationToken ct);

    Task<OfferDto?> GetAsync(Guid id, CancellationToken ct);
}
