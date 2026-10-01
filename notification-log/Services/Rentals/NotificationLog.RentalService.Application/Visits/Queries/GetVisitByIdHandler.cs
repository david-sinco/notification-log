using System.Security.Claims;
using Application.Shared.Common;
using Domain.Shared.Authorization;
using Domain.Shared.Common;
using NotificationLog.RentalService.Application.Visits.Queries.Dtos;
using NotificationLog.RentalService.Domain.Visits;

namespace NotificationLog.RentalService.Application.Visits.Queries;

public sealed class GetVisitByIdHandler(IVisitReadModel visits)
{
    private readonly IVisitReadModel _visits = visits;

    public async Task<VisitDto> HandleAsync(Guid id, ClaimsPrincipal user, CancellationToken ct)
    {
        var visit = await _visits.GetAsync(id, ct) ?? throw new NotFoundException(nameof(Visit), id);
        var userId = user.GetUserId();

        if (!user.IsAdministrador() && !user.IsModerador() && visit.HostId != userId && visit.VisitorId != userId)
            throw new ForbiddenException("Solo puedes consultar las visitas en las que participas.");

        return visit;
    }
}
