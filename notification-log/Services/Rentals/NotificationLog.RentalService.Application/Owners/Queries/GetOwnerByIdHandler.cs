using System.Security.Claims;
using Application.Shared.Common;
using Domain.Shared.Authorization;
using Domain.Shared.Common;
using NotificationLog.RentalService.Application.Owners.Queries.Dtos;
using NotificationLog.RentalService.Domain.Owners;

namespace NotificationLog.RentalService.Application.Owners.Queries;

public sealed class GetOwnerByIdHandler(IOwnerReadModel owners)
{
    private readonly IOwnerReadModel _owners = owners;

    public async Task<OwnerDto> HandleAsync(Guid id, ClaimsPrincipal user, CancellationToken ct)
    {
        var owner = await _owners.GetAsync(id, ct) ?? throw new NotFoundException(nameof(Owner), id);

        var userId = user.GetUserId();

        if (!user.IsAdministrador() && !user.IsModerador() && owner.CreatedBy != userId && owner.RelatedUserId != userId)
            throw new ForbiddenException("Solo puedes consultar tus propietarios o los que registraste.");

        return owner;
    }
}
