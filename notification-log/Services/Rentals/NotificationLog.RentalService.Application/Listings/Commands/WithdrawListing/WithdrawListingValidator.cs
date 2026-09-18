using FluentValidation;
using NotificationLog.RentalService.Domain.Listings;

namespace NotificationLog.RentalService.Application.Listings.Commands.WithdrawListing;

internal sealed class WithdrawListingValidator : AbstractValidator<WithdrawListingCommand>
{
    public WithdrawListingValidator()
    {
        RuleFor(x => x.ListingId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(ListingPolicy.MaxReasonLength);
    }
}
