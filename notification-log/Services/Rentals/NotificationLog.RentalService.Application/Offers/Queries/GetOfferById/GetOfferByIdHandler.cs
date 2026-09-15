using Application.Shared.Common;
using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Application.Offers.Dtos;
using NotificationLog.RentalService.Domain.Offers;

namespace NotificationLog.RentalService.Application.Offers.Queries.GetOfferById;

public sealed class GetOfferByIdHandler
{
    private readonly IOfferReadModel _offers;

    public GetOfferByIdHandler(IOfferReadModel offers) => _offers = offers;

    public async Task<OfferDto> HandleAsync(GetOfferByIdQuery query, CancellationToken ct)
        => await _offers.GetAsync(query.Id, ct) ?? throw new NotFoundException(nameof(Offer), query.Id);
}
