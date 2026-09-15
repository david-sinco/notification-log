using Application.Shared.Abstractions;
using NotificationLog.RentalService.Domain.Visits;

namespace NotificationLog.RentalService.Application.Visits.Commands.AutoCompleteVisit;

public sealed class AutoCompleteVisitHandler
{
    private readonly IVisitRepository _visits;
    private readonly IUnitOfWork _uow;
    private readonly TimeProvider _time;

    public AutoCompleteVisitHandler(IVisitRepository visits, IUnitOfWork uow, TimeProvider time)
        => (_visits, _uow, _time) = (visits, uow, time);

    public async Task HandleAsync(AutoCompleteVisitCommand cmd, CancellationToken ct)
    {
        if (await _visits.LoadAsync(cmd.VisitId, ct) is not { } visit)
            return;

        visit.AutoComplete(_time.GetUtcNow());

        await _visits.AppendAsync(visit, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
