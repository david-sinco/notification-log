using Application.Shared.Common;
using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Application.Visits.Dtos;
using NotificationLog.RentalService.Domain.Visits;

namespace NotificationLog.RentalService.Application.Visits.Queries.GetVisitById;

public sealed class GetVisitByIdHandler
{
    private readonly IVisitReadModel _visits;

    public GetVisitByIdHandler(IVisitReadModel visits) => _visits = visits;

    public async Task<VisitDto> HandleAsync(GetVisitByIdQuery query, CancellationToken ct)
        => await _visits.GetAsync(query.Id, ct) ?? throw new NotFoundException(nameof(Visit), query.Id);
}
