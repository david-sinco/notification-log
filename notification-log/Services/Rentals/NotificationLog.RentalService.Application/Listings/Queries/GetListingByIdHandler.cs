using Application.Shared.Common;
using NotificationLog.RentalService.Application.Listings.Queries.Dtos;
using NotificationLog.RentalService.Domain.Listings;

namespace NotificationLog.RentalService.Application.Listings.Queries;

public sealed class GetListingByIdHandler
{
    private readonly IListingReadModel _listings;

    public GetListingByIdHandler(IListingReadModel listings) => _listings = listings;

    public async Task<ListingDto> HandleAsync(Guid id, CancellationToken ct)
        => await _listings.GetAsync(id, ct) ?? throw new NotFoundException(nameof(Listing), id);
}
