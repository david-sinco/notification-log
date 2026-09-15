using FluentValidation;
using NotificationLog.RentalService.Domain.Offers;

namespace NotificationLog.RentalService.Application.Offers.Commands.SubmitOffer;

internal sealed class SubmitOfferValidator : AbstractValidator<SubmitOfferCommand>
{
    public SubmitOfferValidator()
    {
        RuleFor(x => x.OffererId).NotEmpty();
        RuleFor(x => x.ListingId).NotEmpty();
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.RentTermMonths)
            .InclusiveBetween(OfferPolicy.MinRentMonths, OfferPolicy.MaxRentMonths)
            .When(x => x.RentTermMonths.HasValue);
        RuleFor(x => x)
            .Must(x => x.RentStartDate.HasValue == x.RentTermMonths.HasValue)
            .WithMessage("La fecha de inicio y la duración del arriendo se indican juntas.");
    }
}
