using System.Security.Claims;
using Application.Shared.Common;
using Domain.Shared.Authorization;
using NotificationLog.RentalService.Domain.Owners;

namespace NotificationLog.RentalService.Application.Owners.Commands;

internal static class OwnerIdResolver
{
    public static async Task<Guid> ResolveAsync(IOwnerRepository owners, ClaimsPrincipal user, CancellationToken ct)
    {
        if (user.IsAdministrador() || user.IsAsesor())
            return Guid.NewGuid();

        if (!user.IsPropietario())
            throw new ForbiddenException("Solo un administrador, un asesor o un propietario pueden registrar propietarios.");

        var userId = user.GetUserId();

        if (await owners.LoadAsync(userId, ct) is not null)
            throw new AppValidationException("Ya estás registrado como propietario.");

        return userId;
    }
}
