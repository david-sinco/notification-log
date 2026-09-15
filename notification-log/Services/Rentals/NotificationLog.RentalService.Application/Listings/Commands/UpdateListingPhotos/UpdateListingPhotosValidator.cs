using FluentValidation;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.ValueObjects;

namespace NotificationLog.RentalService.Application.Listings.Commands.UpdateListingPhotos;

internal sealed class UpdateListingPhotosValidator : AbstractValidator<UpdateListingPhotosCommand>
{
    public UpdateListingPhotosValidator()
    {
        RuleFor(x => x.ActorId).NotEmpty();
        RuleFor(x => x.ListingId).NotEmpty();
        RuleFor(x => x.Photos).NotNull().Must(photos => photos.Count <= ListingPolicy.MaxPhotos)
            .WithMessage($"Una publicación no puede tener más de {ListingPolicy.MaxPhotos} fotos.");
        RuleForEach(x => x.Photos).NotEmpty().MaximumLength(Photo.MaxReferenceLength);
    }
}
