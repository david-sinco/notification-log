using Application.Shared.Abstractions;
using Application.Shared.Common;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Offers;

namespace NotificationLog.RentalService.Application.Offers.Commands.WithdrawOffer;

public sealed class WithdrawOfferHandler
{
    private readonly IOfferRepository _offers;
    private readonly IUnitOfWork _uow;

    public WithdrawOfferHandler(IOfferRepository offers, IUnitOfWork uow)
        => (_offers, _uow) = (offers, uow);

    public async Task HandleAsync(WithdrawOfferCommand cmd, CancellationToken ct)
    {
        var offer = await _offers.GetAsync(cmd.OfferId, ct);

        if (offer.OffererId != cmd.ActorId)
            throw new AppValidationException("Solo quien hizo la oferta puede retirarla.");

        offer.Withdraw();

        await _offers.AppendAsync(offer, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
