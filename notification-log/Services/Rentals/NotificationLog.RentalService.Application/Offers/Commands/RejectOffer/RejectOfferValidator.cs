using FluentValidation;
using NotificationLog.RentalService.Domain.Offers;

namespace NotificationLog.RentalService.Application.Offers.Commands.RejectOffer;

internal sealed class RejectOfferValidator : AbstractValidator<RejectOfferCommand>
{
    public RejectOfferValidator()
    {
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.OfferId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(OfferPolicy.MaxReasonLength);
    }
}
