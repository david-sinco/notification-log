using System.Security.Claims;
using Application.Shared.Abstractions;
using Application.Shared.Common;
using Domain.Shared.Authorization;
using Domain.Shared.Common;
using FluentValidation;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Owners;

namespace NotificationLog.RentalService.Application.Listings.Commands.DraftListing;

public sealed class DraftListingHandler(
    IListingRepository listings,
    IOwnerRepository owners,
    IUnitOfWork uow,
    IValidator<DraftListingCommand> validator)
{
    private readonly IListingRepository _listings = listings;
    private readonly IOwnerRepository _owners = owners;
    private readonly IUnitOfWork _uow = uow;
    private readonly IValidator<DraftListingCommand> _validator = validator;

    public async Task<Guid> HandleAsync(DraftListingCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var userId = user.GetUserId();

        if (user.IsPropietario() && cmd.OwnerId != userId)
            throw new ForbiddenException("Un propietario solo puede crear publicaciones a su nombre.");

        if (await _owners.LoadAsync(cmd.OwnerId, ct) is null)
            throw new AppValidationException("El propietario no está registrado.");

        var listing = Listing.Draft(Guid.NewGuid(), cmd.OwnerId, userId, cmd.Operation);
        await _listings.AppendAsync(listing, ct);

        await _uow.SaveChangesAsync(ct);

        return listing.Id;
    }
}
