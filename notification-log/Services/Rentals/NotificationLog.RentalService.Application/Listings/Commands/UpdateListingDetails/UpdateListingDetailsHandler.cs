using Application.Shared.Abstractions;
using Application.Shared.Common;
using FluentValidation;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.ValueObjects;
using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Application.Listings.Commands.UpdateListingDetails;

public sealed class UpdateListingDetailsHandler
{
    private readonly IListingRepository _listings;
    private readonly ListingAccess _access;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<UpdateListingDetailsCommand> _validator;

    public UpdateListingDetailsHandler(
        IListingRepository listings, ListingAccess access, IUnitOfWork uow, IValidator<UpdateListingDetailsCommand> validator)
        => (_listings, _access, _uow, _validator) = (listings, access, uow, validator);

    public async Task HandleAsync(UpdateListingDetailsCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var listing = await _listings.GetAsync(cmd.ListingId, ct);
        await _access.EnsureCanManageAsync(listing, cmd.ActorId, ct);

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
