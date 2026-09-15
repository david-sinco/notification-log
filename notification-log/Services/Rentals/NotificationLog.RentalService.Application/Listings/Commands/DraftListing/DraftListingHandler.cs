using Application.Shared.Abstractions;
using Application.Shared.Common;
using FluentValidation;
using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Domain.Listings;

namespace NotificationLog.RentalService.Application.Listings.Commands.DraftListing;

public sealed class DraftListingHandler
{
    private readonly IListingRepository _listings;
    private readonly IIdentityReplica _identity;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<DraftListingCommand> _validator;

    public DraftListingHandler(
        IListingRepository listings, IIdentityReplica identity, IUnitOfWork uow, IValidator<DraftListingCommand> validator)
        => (_listings, _identity, _uow, _validator) = (listings, identity, uow, validator);

    public async Task<Guid> HandleAsync(DraftListingCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        if (cmd.AdvisorId is { } advisorId)
        {
            if (advisorId != cmd.ActorId)
                throw new AppValidationException("Solo el propio asesor puede crear publicaciones a su nombre.");

            if (await _identity.GetAdvisorAsync(advisorId, ct) is not { IsActive: true })
                throw new AppValidationException("El asesor no está activo.");
        }
        else if (await _identity.GetPersonByUserAsync(cmd.ActorId, ct) is not { } person || person.PersonId != cmd.PublisherId)
        {
            throw new AppValidationException("Solo el propietario o su asesor pueden crear la publicación.");
        }

        var listing = Listing.Draft(Guid.NewGuid(), cmd.PublisherId, cmd.ActorId, cmd.AdvisorId, cmd.Operation);

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);

        return listing.Id;
    }
}
