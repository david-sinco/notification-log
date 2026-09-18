using FluentValidation;

namespace NotificationLog.RentalService.Application.Listings.Commands.DraftListing;

internal sealed class DraftListingValidator : AbstractValidator<DraftListingCommand>
{
    public DraftListingValidator()
    {
        RuleFor(x => x.OwnerId).NotEmpty();
        RuleFor(x => x.Operation).IsInEnum();
    }
}
