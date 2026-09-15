using FluentValidation;

namespace NotificationLog.RentalService.Application.Listings.Commands.ChangeListingPrice;

internal sealed class ChangeListingPriceValidator : AbstractValidator<ChangeListingPriceCommand>
{
    public ChangeListingPriceValidator()
    {
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.ListingId).NotEmpty();
        RuleFor(x => x.Price).GreaterThan(0);
    }
}
