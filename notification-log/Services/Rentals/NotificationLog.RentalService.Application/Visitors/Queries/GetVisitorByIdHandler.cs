using System.Security.Claims;
using Application.Shared.Common;
using Domain.Shared.Authorization;
using Domain.Shared.Common;
using NotificationLog.RentalService.Application.Visitors.Queries.Dtos;
using NotificationLog.RentalService.Domain.Visitors;

namespace NotificationLog.RentalService.Application.Visitors.Queries;

public sealed class GetVisitorByIdHandler(IVisitorReadModel visitors)
{
    private readonly IVisitorReadModel _visitors = visitors;

    public async Task<VisitorDto> HandleAsync(Guid id, ClaimsPrincipal user, CancellationToken ct)
    {
        if (!user.IsAdministrador() && !user.IsModerador() && id != user.GetUserId())
            throw new ForbiddenException("Solo puedes consultar tu propio registro de visitante.");

        return await _visitors.GetAsync(id, ct) ?? throw new NotFoundException(nameof(Visitor), id);
    }
}
