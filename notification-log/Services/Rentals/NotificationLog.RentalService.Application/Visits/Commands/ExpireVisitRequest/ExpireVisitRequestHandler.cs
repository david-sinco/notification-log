using Application.Shared.Abstractions;
using NotificationLog.RentalService.Domain.Visits;

namespace NotificationLog.RentalService.Application.Visits.Commands.ExpireVisitRequest;

public sealed class ExpireVisitRequestHandler
{
    private readonly IVisitRepository _visits;
    private readonly IUnitOfWork _uow;
    private readonly TimeProvider _time;

    public ExpireVisitRequestHandler(IVisitRepository visits, IUnitOfWork uow, TimeProvider time)
        => (_visits, _uow, _time) = (visits, uow, time);

    public async Task HandleAsync(ExpireVisitRequestCommand cmd, CancellationToken ct)
    {
        if (await _visits.LoadAsync(cmd.VisitId, ct) is not { } visit)
            return;

        visit.ExpireRequest(_time.GetUtcNow());

        await _visits.AppendAsync(visit, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
