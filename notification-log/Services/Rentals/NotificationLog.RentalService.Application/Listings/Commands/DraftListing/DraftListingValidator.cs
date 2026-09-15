using FluentValidation;

namespace NotificationLog.RentalService.Application.Listings.Commands.DraftListing;

internal sealed class DraftListingValidator : AbstractValidator<DraftListingCommand>
{
    public DraftListingValidator()
    {
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.PublisherId).NotEmpty();
        RuleFor(x => x.AdvisorId).NotEqual(Guid.Empty).When(x => x.AdvisorId.HasValue);
        RuleFor(x => x.Operation).IsInEnum();
    }
}
