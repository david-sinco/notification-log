using Application.Shared.Common;
using NotificationLog.RentalService.Application.Common.Storage;
using NotificationLog.RentalService.Application.Listings.Queries.Dtos;
using NotificationLog.RentalService.Domain.Listings;

namespace NotificationLog.RentalService.Application.Listings.Queries;

public sealed class GetListingByIdHandler
{
    private readonly IListingReadModel _listings;
    private readonly IPhotoUrlProvider _photoUrls;

    public GetListingByIdHandler(IListingReadModel listings, IPhotoUrlProvider photoUrls)
    {
        _listings = listings;
        _photoUrls = photoUrls;
    }

    public async Task<ListingDto> HandleAsync(Guid id, CancellationToken ct)
    {
        var listing = await _listings.GetAsync(id, ct) ?? throw new NotFoundException(nameof(Listing), id);

        return listing.WithPhotoUrls(_photoUrls);
    }
}
