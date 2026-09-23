using Application.Shared.Pagination;
using Marten;
using Marten.Linq;
using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Application.Listings.Dtos;
using NotificationLog.RentalService.Application.Listings.Queries.ListListings;

namespace NotificationLog.RentalService.Infrastructure.ReadModels;

internal sealed class MartenListingReadModel : IListingReadModel
{
    private readonly IQuerySession _session;

    public MartenListingReadModel(IQuerySession session) => _session = session;

    public async Task<(IReadOnlyList<ListingSummaryDto> Items, int TotalCount)> ListAsync(ListListingsQuery query, PageRequest paging, CancellationToken ct)
    {
        IQueryable<ListingView> listings = _session.Query<ListingView>();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var search = query.Search.Trim();
            listings = listings.Where(x =>
                x.City!.Contains(search, StringComparison.OrdinalIgnoreCase)
                || x.Neighborhood!.Contains(search, StringComparison.OrdinalIgnoreCase)
                || x.Address!.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (query.Status is { } status)
            listings = listings.Where(x => x.Status == status);

        if (query.Operation is { } operation)
            listings = listings.Where(x => x.Operation == operation);

        if (query.ParticipantId is { } participant)
            listings = listings.Where(x =>
                x.OwnerId == participant || x.CreatedBy == participant);

        var items = await ((IMartenQueryable<ListingView>)listings)
            .Stats(out QueryStatistics stats)
            .OrderByDescending(x => x.UpdatedAt)
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .ToListAsync(ct);

        return (items.Select(ToSummary).ToList(), (int)stats.TotalResults);
    }

    public async Task<ListingDto?> GetAsync(Guid id, CancellationToken ct)
        => await _session.LoadAsync<ListingView>(id, ct) is { } x
            ? new ListingDto(
                x.Id,
                x.OwnerId,
                x.CreatedBy,
                x.Operation.ToString(),
                x.Status.ToString(),
                x.Type?.ToString(),
                x.Area,
                x.Bedrooms,
                x.Bathrooms,
                x.ParkingSpots,
                x.Stratum,
                x.Floor,
                x.HasElevator,
                x.AdministrationFee,
                x.City,
                x.Neighborhood,
                x.Address,
                x.Description,
                x.Price,
                x.Photos,
                x.ExpiresAt,
                x.RejectionReasons.Select(r => r.ToString()).ToList(),
                x.StatusReason,
                x.FinalPrice,
                x.SignedOn,
                x.CreatedAt,
                x.UpdatedAt)
            : null;

    internal static ListingSummaryDto ToSummary(ListingView x) =>
        new(
            x.Id,
            x.Operation.ToString(),
            x.Status.ToString(),
            x.Type?.ToString(),
            x.City,
            x.Neighborhood,
            x.Price,
            x.Bedrooms,
            x.Area,
            x.OwnerId,
            x.CreatedBy,
            x.UpdatedAt);
}
