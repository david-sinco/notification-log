using System.Security.Claims;
using Application.Shared.Common;
using Domain.Shared.Authorization;
using Domain.Shared.Common;
using NotificationLog.RentalService.Domain.Owners;

namespace NotificationLog.RentalService.Application.Owners.Commands;

internal static class RelatedUserResolver
{
    public static async Task<Guid?> ResolveAsync(
        IOwnerRepository owners, Guid ownerId, ClaimsPrincipal user, CancellationToken ct)
    {
        if (user.IsAdministrador() || user.IsModerador())
            return null;

        if (!user.IsPropietario())
            throw new ForbiddenException("Solo un administrador, un moderador o un propietario pueden registrar propietarios.");

        var userId = user.GetUserId();

        if (!await owners.TryReserveUserAsync(ownerId, userId, ct))
            throw new AppValidationException("Ya estás registrado como propietario.");

        return userId;
    }
}
