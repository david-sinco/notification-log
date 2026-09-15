using Application.Shared.Common;
using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Domain.Listings;

namespace NotificationLog.RentalService.Application.Common;

public sealed class ListingAccess
{
    private readonly IIdentityReplica _identity;

    public ListingAccess(IIdentityReplica identity) => _identity = identity;

    public async Task<bool> CanManageAsync(Listing listing, Guid actorId, CancellationToken ct)
    {
        if (listing.AdvisorId == actorId)
            return true;

        var person = await _identity.GetPersonByUserAsync(actorId, ct);

        return person is not null && person.PersonId == listing.PublisherId;
    }

    public async Task EnsureCanManageAsync(Listing listing, Guid actorId, CancellationToken ct)
    {
        if (!await CanManageAsync(listing, actorId, ct))
            throw new AppValidationException("No tienes permiso para gestionar esta publicación.");
    }

    public async Task<PersonVerification> RequireVerifiedUserAsync(Guid userId, bool requireDocument, CancellationToken ct)
    {
        var person = await _identity.GetPersonByUserAsync(userId, ct)
            ?? throw new AppValidationException("El usuario no está registrado en la plataforma.");

        if (!person.IsPhoneVerified)
            throw new AppValidationException("Necesitas verificar tu teléfono para hacer esto.");

        if (requireDocument && !person.IsDocumentVerified)
            throw new AppValidationException("Necesitas verificar tu documento para hacer esto.");

        return person;
    }

    public static Guid HostOf(Listing listing) => listing.AdvisorId ?? listing.CreatedBy;
}
