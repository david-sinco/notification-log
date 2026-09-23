using Application.Shared.Pagination;
using NotificationLog.RentalService.Application.Listings.Queries.Dtos;
using NotificationLog.RentalService.Application.Listings.Queries.Filters;

namespace NotificationLog.RentalService.Application.Listings.Queries;

public interface IListingReadModel
{
    Task<(IReadOnlyList<ListingSummaryDto> Items, int TotalCount)> ListAsync(ListingFilter filter, PageRequest paging, CancellationToken ct);

    Task<ListingDto?> GetAsync(Guid id, CancellationToken ct);
}
