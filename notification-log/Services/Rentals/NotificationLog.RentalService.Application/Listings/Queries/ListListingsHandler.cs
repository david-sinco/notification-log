using Application.Shared.Pagination;
using NotificationLog.RentalService.Application.Common.Storage;
using NotificationLog.RentalService.Application.Listings.Queries.Dtos;
using NotificationLog.RentalService.Application.Listings.Queries.Filters;

namespace NotificationLog.RentalService.Application.Listings.Queries;

public sealed class ListListingsHandler
{
    private readonly IListingReadModel _listings;
    private readonly IPhotoUrlProvider _photoUrls;

    public ListListingsHandler(IListingReadModel listings, IPhotoUrlProvider photoUrls)
    {
        _listings = listings;
        _photoUrls = photoUrls;
    }

    public async Task<PagedResult<ListingSummaryDto>> HandleAsync(ListingFilter filter, PageRequest paging, CancellationToken ct)
    {
        var (items, total) = await _listings.ListAsync(filter, paging, ct);

        return new PagedResult<ListingSummaryDto>([.. items.Select(x => x.WithCoverUrl(_photoUrls))], paging, total);
    }
}
