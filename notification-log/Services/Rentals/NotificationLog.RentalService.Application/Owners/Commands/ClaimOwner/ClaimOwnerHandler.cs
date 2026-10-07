using System.Security.Claims;
using Application.Shared.Abstractions;
using Application.Shared.Common;
using Domain.Shared.Authorization;
using Domain.Shared.Common;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Owners;

namespace NotificationLog.RentalService.Application.Owners.Commands.ClaimOwner;

public sealed class ClaimOwnerHandler(IOwnerRepository owners, IUnitOfWork uow)
{
    private readonly IOwnerRepository _owners = owners;
    private readonly IUnitOfWork _uow = uow;

    public async Task HandleAsync(ClaimOwnerCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        if (!user.IsPropietario())
            throw new ForbiddenException("Solo un propietario puede reclamar un propietario.");

        var userId = user.GetUserId();
        var owner = await _owners.GetAsync(cmd.OwnerId, ct);

        owner.Claim(userId, user.GetEmail(), user.GetPhone());

        if (!await _owners.TryReserveUserAsync(owner.Id, userId, ct))
            throw new AppValidationException("Ya estás registrado como propietario.");

        await _owners.AppendAsync(owner, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
