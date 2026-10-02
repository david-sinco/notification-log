using Application.Shared.Pagination;
using Marten;
using Marten.Linq;
using NotificationLog.RentalService.Application.Common.Storage;
using NotificationLog.RentalService.Application.Listings.Queries;
using NotificationLog.RentalService.Application.Listings.Queries.Dtos;
using NotificationLog.RentalService.Application.Listings.Queries.Filters;

namespace NotificationLog.RentalService.Infrastructure.ReadModels;

internal sealed class MartenListingReadModel : IListingReadModel
{
    private readonly IQuerySession _session;
    private readonly IPhotoUrlProvider _photoUrls;

    public MartenListingReadModel(IQuerySession session, IPhotoUrlProvider photoUrls)
    {
        _session = session;
        _photoUrls = photoUrls;
    }

    public async Task<(IReadOnlyList<ListingSummaryDto> Items, int TotalCount)> ListAsync(ListingFilter filter, PageRequest paging, CancellationToken ct)
    {
        IQueryable<ListingView> listings = _session.Query<ListingView>();

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var search = filter.Search.Trim();
            listings = listings.Where(x =>
                x.City!.Contains(search, StringComparison.OrdinalIgnoreCase)
                || x.Neighborhood!.Contains(search, StringComparison.OrdinalIgnoreCase)
                || x.Address!.Contains(search, StringComparison.OrdinalIgnoreCase));
        }

        if (filter.Status is { } status)
            listings = listings.Where(x => x.Status == status);

        if (filter.Operation is { } operation)
            listings = listings.Where(x => x.Operation == operation);

        if (filter.ParticipantId is { } participant)
            listings = listings.Where(x =>
                x.OwnerId == participant || x.CreatedBy == participant);

        var items = await ((IMartenQueryable<ListingView>)listings)
            .Stats(out QueryStatistics stats)
            .OrderByDescending(x => x.UpdatedAt)
            .Skip(paging.Skip)
            .Take(paging.PageSize)
            .ToListAsync(ct);

        var owners = await _session.LoadManyAsync<OwnerView>(ct, items.Select(x => x.OwnerId).Distinct().ToArray());
        var ownerNames = owners.ToDictionary(x => x.Id, x => x.DisplayName());

        return (items.Select(x => ToSummary(x, ownerNames.GetValueOrDefault(x.OwnerId))).ToList(), (int)stats.TotalResults);
    }

    public async Task<ListingDto?> GetAsync(Guid id, CancellationToken ct)
    {
        if (await _session.LoadAsync<ListingView>(id, ct) is not { } x)
            return null;

        var owner = await _session.LoadAsync<OwnerView>(x.OwnerId, ct);

        return new ListingDto(
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
            x.Photos.Select(fileName => new ListingPhotoDto(fileName, _photoUrls.ReadUrlFor(x.Id, fileName))).ToList(),
            x.ExpiresAt,
            x.RejectionReasons.Select(r => r.ToString()).ToList(),
            x.StatusReason,
            x.FinalPrice,
            x.SignedOn,
            x.CreatedAt,
            x.UpdatedAt,
            owner?.DisplayName());
    }

    private ListingSummaryDto ToSummary(ListingView x, string? ownerName) =>
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
            x.UpdatedAt,
            ownerName,
            x.Photos.Count > 0 ? _photoUrls.ReadUrlFor(x.Id, x.Photos[0]) : null,
            x.Photos.Count);
}
