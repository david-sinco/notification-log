using System.Security.Claims;
using Application.Shared.Pagination;
using Domain.Shared.Authorization;
using NotificationLog.RentalService.Application.Visits.Queries.Dtos;
using NotificationLog.RentalService.Application.Visits.Queries.Filters;

namespace NotificationLog.RentalService.Application.Visits.Queries;

public sealed class ListVisitsHandler(IVisitReadModel visits)
{
    private readonly IVisitReadModel _visits = visits;

    public async Task<PagedResult<VisitDto>> HandleAsync(
        VisitFilter filter, PageRequest paging, ClaimsPrincipal user, CancellationToken ct)
    {
        if (!user.IsAdministrador() && !user.IsModerador())
            filter = filter with { ParticipantId = user.GetUserId() };

        var (items, total) = await _visits.ListAsync(filter, paging, ct);

        return new PagedResult<VisitDto>(items, paging, total);
    }
}
