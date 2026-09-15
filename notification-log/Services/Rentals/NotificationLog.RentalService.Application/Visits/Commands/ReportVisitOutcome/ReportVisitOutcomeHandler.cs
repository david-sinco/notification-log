using Application.Shared.Abstractions;
using Application.Shared.Common;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Visits;

namespace NotificationLog.RentalService.Application.Visits.Commands.ReportVisitOutcome;

public sealed class ReportVisitOutcomeHandler
{
    private readonly IVisitRepository _visits;
    private readonly IUnitOfWork _uow;
    private readonly TimeProvider _time;

    public ReportVisitOutcomeHandler(IVisitRepository visits, IUnitOfWork uow, TimeProvider time)
        => (_visits, _uow, _time) = (visits, uow, time);

    public async Task HandleAsync(ReportVisitOutcomeCommand cmd, CancellationToken ct)
    {
        var visit = await _visits.GetAsync(cmd.VisitId, ct);

        if (visit.HostId != cmd.ActorId)
            throw new AppValidationException("Solo el anfitrión registra el resultado de la visita.");

        var now = _time.GetUtcNow();

        if (cmd.Attended) visit.MarkCompleted(now);
        else visit.MarkNoShow(now);

        await _visits.AppendAsync(visit, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
