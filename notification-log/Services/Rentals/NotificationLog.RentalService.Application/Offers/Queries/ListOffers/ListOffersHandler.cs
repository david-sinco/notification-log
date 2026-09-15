using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Application.Common;

namespace NotificationLog.RentalService.Application.Offers.Queries.ListOffers;

public sealed class ListOffersHandler
{
    private readonly IOfferReadModel _offers;

    public ListOffersHandler(IOfferReadModel offers) => _offers = offers;

    public async Task<PagedResult<OfferSummaryDto>> HandleAsync(ListOffersQuery query, CancellationToken ct)
    {
        var paged = query with { Page = Paging.Page(query.Page), PageSize = Paging.Size(query.PageSize) };
        var (items, total) = await _offers.ListAsync(paged, ct);

        return new PagedResult<OfferSummaryDto>(items, paged.Page, paged.PageSize, total);
    }
}
