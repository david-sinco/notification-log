using System.Security.Claims;
using Application.Shared.Abstractions;
using Application.Shared.Common;
using FluentValidation;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Common.ValueObjects;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.ValueObjects;

namespace NotificationLog.RentalService.Application.Listings.Commands.UpdateListingDetails;

public sealed class UpdateListingDetailsHandler(
    IListingRepository listings,
    IUnitOfWork uow,
    IValidator<UpdateListingDetailsCommand> validator)
{
    private readonly IListingRepository _listings = listings;
    private readonly IUnitOfWork _uow = uow;
    private readonly IValidator<UpdateListingDetailsCommand> _validator = validator;

    public async Task HandleAsync(UpdateListingDetailsCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var listing = await _listings.GetAsync(cmd.ListingId, ct);
        ListingAccess.EnsureCanManage(listing, user);

        var details = PropertyDetails.Create(
            cmd.Type,
            cmd.Area,
            cmd.Bedrooms,
            cmd.Bathrooms,
            cmd.ParkingSpots,
            Stratum.Create(cmd.Stratum),
            cmd.Floor,
            cmd.HasElevator,
            Money.Create(cmd.AdministrationFee));

        listing.UpdateDetails(
            details,
            Location.Create(cmd.City, cmd.Neighborhood, cmd.Address),
            ListingDescription.Create(cmd.Description));

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
