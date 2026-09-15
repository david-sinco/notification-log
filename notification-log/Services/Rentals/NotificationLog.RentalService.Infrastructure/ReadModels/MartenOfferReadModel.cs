using Marten;
using Marten.Linq;
using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Application.Offers.Dtos;
using NotificationLog.RentalService.Application.Offers.Queries.ListOffers;

namespace NotificationLog.RentalService.Infrastructure.ReadModels;

internal sealed class MartenOfferReadModel : IOfferReadModel
{
    private readonly IQuerySession _session;

    public MartenOfferReadModel(IQuerySession session) => _session = session;

    public async Task<(IReadOnlyList<OfferSummaryDto> Items, int TotalCount)> ListAsync(ListOffersQuery query, CancellationToken ct)
    {
        IQueryable<OfferView> offers = _session.Query<OfferView>();

        if (query.ListingId is { } listingId)
            offers = offers.Where(x => x.ListingId == listingId);

        if (query.OffererId is { } offererId)
            offers = offers.Where(x => x.OffererId == offererId);

        if (query.Status is { } status)
            offers = offers.Where(x => x.Status == status);

        var items = await ((IMartenQueryable<OfferView>)offers)
            .Stats(out QueryStatistics stats)
            .OrderByDescending(x => x.UpdatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(ct);

        return (items.Select(x => new OfferSummaryDto(
                x.Id,
                x.ListingId,
                x.OffererId,
                x.Operation.ToString(),
                x.Status.ToString(),
                x.ListedPrice,
                x.Amount,
                x.LastMoveBy.ToString(),
                x.CounterOfferCount,
                x.RespondBy,
                x.UpdatedAt))
            .ToList(), (int)stats.TotalResults);
    }

    public async Task<OfferDto?> GetAsync(Guid id, CancellationToken ct)
        => await _session.LoadAsync<OfferView>(id, ct) is { } x
            ? new OfferDto(
                x.Id,
                x.ListingId,
                x.OffererId,
                x.Operation.ToString(),
                x.Status.ToString(),
                x.ListedPrice,
                x.Amount,
                x.RentStartDate,
                x.RentTermMonths,
                x.LastMoveBy.ToString(),
                x.CounterOfferCount,
                x.RespondBy,
                x.SubmittedAt,
                x.UpdatedAt,
                x.History.Select(m => new OfferMoveDto(m.Kind, m.By?.ToString(), m.Amount, m.Reason, m.At)).ToList())
            : null;
}
