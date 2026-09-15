using FluentValidation;

namespace NotificationLog.RentalService.Application.Listings.Commands.ReportListing;

internal sealed class ReportListingValidator : AbstractValidator<ReportListingCommand>
{
    public ReportListingValidator()
    {
        RuleFor(x => x.ReporterId).NotEmpty();
        RuleFor(x => x.ListingId).NotEmpty();
        RuleFor(x => x.Reason).IsInEnum();
    }
}
