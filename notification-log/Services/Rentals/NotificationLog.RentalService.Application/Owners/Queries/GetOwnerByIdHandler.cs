using System.Security.Claims;
using Application.Shared.Common;
using Domain.Shared.Authorization;
using NotificationLog.RentalService.Application.Owners.Queries.Dtos;
using NotificationLog.RentalService.Domain.Owners;

namespace NotificationLog.RentalService.Application.Owners.Queries;

public sealed class GetOwnerByIdHandler
{
    private readonly IOwnerReadModel _owners;

    public GetOwnerByIdHandler(IOwnerReadModel owners) => _owners = owners;

    public async Task<OwnerDto> HandleAsync(Guid id, ClaimsPrincipal user, CancellationToken ct)
    {
        var owner = await _owners.GetAsync(id, ct) ?? throw new NotFoundException(nameof(Owner), id);

        if (!user.IsAdministrador() && !user.IsModerador() && owner.CreatedBy != user.GetUserId())
            throw new ForbiddenException("Solo puedes consultar los propietarios que registraste.");

        return owner;
    }
}
