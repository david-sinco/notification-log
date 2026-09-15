using Application.Shared.Abstractions;
using NotificationLog.RentalService.Domain.Offers;

namespace NotificationLog.RentalService.Application.Offers.Commands.ExpireOffer;

public sealed class ExpireOfferHandler
{
    private readonly IOfferRepository _offers;
    private readonly IUnitOfWork _uow;
    private readonly TimeProvider _time;

    public ExpireOfferHandler(IOfferRepository offers, IUnitOfWork uow, TimeProvider time)
        => (_offers, _uow, _time) = (offers, uow, time);

    public async Task HandleAsync(ExpireOfferCommand cmd, CancellationToken ct)
    {
        if (await _offers.LoadAsync(cmd.OfferId, ct) is not { } offer)
            return;

        offer.Expire(_time.GetUtcNow());

        await _offers.AppendAsync(offer, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
