using FluentValidation;

namespace NotificationLog.RentalService.Application.Listings.Commands.ReviewListing;

internal sealed class ReviewListingValidator : AbstractValidator<ReviewListingCommand>
{
    public ReviewListingValidator()
    {
        RuleFor(x => x.ListingId).NotEmpty();
        RuleFor(x => x.Reasons).NotEmpty().When(x => !x.Approve);
        RuleForEach(x => x.Reasons).IsInEnum();
    }
}
