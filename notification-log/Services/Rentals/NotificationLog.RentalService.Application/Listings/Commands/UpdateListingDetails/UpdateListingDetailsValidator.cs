using FluentValidation;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.ValueObjects;
using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Application.Listings.Commands.UpdateListingDetails;

internal sealed class UpdateListingDetailsValidator : AbstractValidator<UpdateListingDetailsCommand>
{
    public UpdateListingDetailsValidator()
    {
        RuleFor(x => x.ListingId).NotEmpty();
        RuleFor(x => x.Type).IsInEnum();
        RuleFor(x => x.Area).GreaterThanOrEqualTo(ListingPolicy.MinArea);
        RuleFor(x => x.Bedrooms).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Bathrooms).GreaterThanOrEqualTo(1);
        RuleFor(x => x.ParkingSpots).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Stratum).InclusiveBetween(Stratum.Min, Stratum.Max);
        RuleFor(x => x.Floor).GreaterThanOrEqualTo(1).When(x => x.Floor.HasValue);
        RuleFor(x => x.AdministrationFee).GreaterThanOrEqualTo(0);
        RuleFor(x => x.City).NotEmpty().MaximumLength(Location.MaxCityLength);
        RuleFor(x => x.Neighborhood).NotEmpty().MaximumLength(Location.MaxNeighborhoodLength);
        RuleFor(x => x.Address).NotEmpty().MaximumLength(Location.MaxAddressLength);
        RuleFor(x => x.Description).NotEmpty().Length(ListingPolicy.MinDescriptionLength, ListingPolicy.MaxDescriptionLength);
    }
}
