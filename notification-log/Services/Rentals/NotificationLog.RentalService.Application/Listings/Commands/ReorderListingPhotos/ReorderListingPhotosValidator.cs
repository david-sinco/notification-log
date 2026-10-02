using FluentValidation;

namespace NotificationLog.RentalService.Application.Listings.Commands.ReorderListingPhotos;

internal sealed class ReorderListingPhotosValidator : AbstractValidator<ReorderListingPhotosCommand>
{
    public ReorderListingPhotosValidator()
    {
        RuleFor(x => x.ListingId).NotEmpty();
        RuleFor(x => x.FileNames).NotNull();
        RuleForEach(x => x.FileNames).NotEmpty();
    }
}
