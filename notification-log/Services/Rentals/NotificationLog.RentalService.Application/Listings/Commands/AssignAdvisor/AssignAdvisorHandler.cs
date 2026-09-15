using Application.Shared.Abstractions;
using Application.Shared.Common;
using FluentValidation;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Application.Abstractions;

namespace NotificationLog.RentalService.Application.Listings.Commands.AssignAdvisor;

public sealed class AssignAdvisorHandler
{
    private readonly IListingRepository _listings;
    private readonly ListingAccess _access;
    private readonly IIdentityReplica _identity;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<AssignAdvisorCommand> _validator;

    public AssignAdvisorHandler(
        IListingRepository listings,
        ListingAccess access,
        IIdentityReplica identity,
        IUnitOfWork uow,
        IValidator<AssignAdvisorCommand> validator)
        => (_listings, _access, _identity, _uow, _validator) = (listings, access, identity, uow, validator);

    public async Task HandleAsync(AssignAdvisorCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var listing = await _listings.GetAsync(cmd.ListingId, ct);
        await _access.EnsureCanManageAsync(listing, cmd.ActorId, ct);

        if (cmd.AdvisorId is not { } advisorId)
        {
            listing.UnassignAdvisor();
        }
        else
        {
            var advisor = await _identity.GetAdvisorAsync(advisorId, ct);

            if (advisor is not { IsActive: true })
                throw new AppValidationException("El asesor no está activo.");

            if (listing.Location is { } location
                && !advisor.ServiceCities.Contains(location.City, StringComparer.OrdinalIgnoreCase))
                throw new AppValidationException($"El asesor no presta servicio en {location.City}.");

            listing.AssignAdvisor(advisorId);
        }

        await _listings.AppendAsync(listing, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
