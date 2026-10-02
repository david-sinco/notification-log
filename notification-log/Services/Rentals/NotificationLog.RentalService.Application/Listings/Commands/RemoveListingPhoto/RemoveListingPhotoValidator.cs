using FluentValidation;

namespace NotificationLog.RentalService.Application.Listings.Commands.RemoveListingPhoto;

internal sealed class RemoveListingPhotoValidator : AbstractValidator<RemoveListingPhotoCommand>
{
    public RemoveListingPhotoValidator()
    {
        RuleFor(x => x.ListingId).NotEmpty();
        RuleFor(x => x.FileName).NotEmpty();
    }
}
