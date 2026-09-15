using Application.Shared.Common;
using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Application.Listings.Dtos;
using NotificationLog.RentalService.Domain.Listings;

namespace NotificationLog.RentalService.Application.Listings.Queries.GetListingById;

public sealed class GetListingByIdHandler
{
    private readonly IListingReadModel _listings;

    public GetListingByIdHandler(IListingReadModel listings) => _listings = listings;

    public async Task<ListingDto> HandleAsync(GetListingByIdQuery query, CancellationToken ct)
        => await _listings.GetAsync(query.Id, ct) ?? throw new NotFoundException(nameof(Listing), query.Id);
}
