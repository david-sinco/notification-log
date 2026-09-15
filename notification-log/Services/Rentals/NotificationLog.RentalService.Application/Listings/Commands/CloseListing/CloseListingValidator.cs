using FluentValidation;

namespace NotificationLog.RentalService.Application.Listings.Commands.CloseListing;

internal sealed class CloseListingValidator : AbstractValidator<CloseListingCommand>
{
    public CloseListingValidator()
    {
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.ListingId).NotEmpty();
        RuleFor(x => x.FinalPrice).GreaterThan(0);
    }
}
