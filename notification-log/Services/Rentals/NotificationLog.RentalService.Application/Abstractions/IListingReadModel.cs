using NotificationLog.RentalService.Application.Listings.Dtos;
using NotificationLog.RentalService.Application.Listings.Queries.ListListings;

namespace NotificationLog.RentalService.Application.Abstractions;

public interface IListingReadModel
{
    Task<(IReadOnlyList<ListingSummaryDto> Items, int TotalCount)> ListAsync(ListListingsQuery query, CancellationToken ct);

    Task<ListingDto?> GetAsync(Guid id, CancellationToken ct);
}
