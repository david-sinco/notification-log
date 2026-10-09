using NotificationLog.RentalService.Application.Common.Storage;
using NotificationLog.RentalService.Application.Listings.Queries.Dtos;

namespace NotificationLog.RentalService.Application.Listings.Queries;

internal static class ListingPhotoUrls
{
    public static ListingDto WithPhotoUrls(this ListingDto listing, IPhotoUrlProvider photoUrls)
        => listing with
        {
            Photos = [.. listing.Photos.Select(photo => photo with { Url = photoUrls.ReadUrlFor(listing.Id, photo.FileName) })]
        };

    public static ListingSummaryDto WithCoverUrl(this ListingSummaryDto listing, IPhotoUrlProvider photoUrls)
        => listing with
        {
            CoverUrl = listing.CoverFileName is { } cover ? photoUrls.ReadUrlFor(listing.Id, cover) : null
        };
}
