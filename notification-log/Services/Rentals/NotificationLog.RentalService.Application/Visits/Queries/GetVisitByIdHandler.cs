using Application.Shared.Common;
using NotificationLog.RentalService.Application.Visits.Queries.Dtos;
using NotificationLog.RentalService.Domain.Visits;

namespace NotificationLog.RentalService.Application.Visits.Queries;

public sealed class GetVisitByIdHandler
{
    private readonly IVisitReadModel _visits;

    public GetVisitByIdHandler(IVisitReadModel visits) => _visits = visits;

    public async Task<VisitDto> HandleAsync(Guid id, CancellationToken ct)
        => await _visits.GetAsync(id, ct) ?? throw new NotFoundException(nameof(Visit), id);
}
