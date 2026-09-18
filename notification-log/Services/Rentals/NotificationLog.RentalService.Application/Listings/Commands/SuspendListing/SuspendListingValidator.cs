using FluentValidation;
using NotificationLog.RentalService.Domain.Listings;

namespace NotificationLog.RentalService.Application.Listings.Commands.SuspendListing;

internal sealed class SuspendListingValidator : AbstractValidator<SuspendListingCommand>
{
    public SuspendListingValidator()
    {
        RuleFor(x => x.ListingId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(ListingPolicy.MaxReasonLength);
    }
}
