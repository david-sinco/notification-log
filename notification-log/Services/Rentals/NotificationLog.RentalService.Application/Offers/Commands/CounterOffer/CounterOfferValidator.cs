using FluentValidation;

namespace NotificationLog.RentalService.Application.Offers.Commands.CounterOffer;

internal sealed class CounterOfferValidator : AbstractValidator<CounterOfferCommand>
{
    public CounterOfferValidator()
    {
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.OfferId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}
