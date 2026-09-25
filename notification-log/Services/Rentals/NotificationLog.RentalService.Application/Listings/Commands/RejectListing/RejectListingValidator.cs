using FluentValidation;

namespace NotificationLog.RentalService.Application.Listings.Commands.RejectListing;

internal sealed class RejectListingValidator : AbstractValidator<RejectListingCommand>
{
    public RejectListingValidator()
    {
        RuleFor(x => x.ListingId).NotEmpty();
        RuleFor(x => x.Reasons).NotEmpty();
        RuleForEach(x => x.Reasons).IsInEnum();
    }
}
