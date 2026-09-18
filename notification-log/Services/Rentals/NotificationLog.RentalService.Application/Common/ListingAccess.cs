using System.Security.Claims;
using Application.Shared.Common;
using Domain.Shared.Authorization;
using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Domain.Listings;

namespace NotificationLog.RentalService.Application.Common;

public sealed class ListingAccess
{
    private readonly IIdentityReplica _identity;

    public ListingAccess(IIdentityReplica identity) => _identity = identity;

    public static bool IsStaff(ClaimsPrincipal user) => user.IsAdministrador() || user.IsModerador();

    public static void EnsureStaff(ClaimsPrincipal user)
    {
        if (!IsStaff(user))
            throw new ForbiddenException("Solo un administrador o un moderador pueden hacer esto.");
    }

    public static void EnsureCanManage(Listing listing, ClaimsPrincipal user)
    {
        if (IsStaff(user))
            return;

        if (!user.IsPropietario() || listing.OwnerId != user.GetUserId())
            throw new ForbiddenException("No tienes permiso para gestionar esta publicación.");
    }

    public static bool IsHost(Listing listing, Guid userId) => listing.OwnerId == userId;

    public static Guid HostOf(Listing listing) => listing.OwnerId;

    public async Task<PersonVerification> RequireVerifiedUserAsync(Guid userId, CancellationToken ct)
    {
        var person = await _identity.GetPersonByUserAsync(userId, ct)
            ?? throw new AppValidationException("El usuario no está registrado en la plataforma.");

        if (!person.IsPhoneVerified)
            throw new AppValidationException("Necesitas verificar tu teléfono para hacer esto.");

        return person;
    }
}
