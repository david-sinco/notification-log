using Application.Shared.Abstractions;
using Application.Shared.Common;
using FluentValidation;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Visits;
using NotificationLog.RentalService.Domain.Visits.Enums;

namespace NotificationLog.RentalService.Application.Visits.Commands.CancelVisit;

public sealed class CancelVisitHandler
{
    private readonly IVisitRepository _visits;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<CancelVisitCommand> _validator;
    private readonly TimeProvider _time;

    public CancelVisitHandler(IVisitRepository visits, IUnitOfWork uow, IValidator<CancelVisitCommand> validator, TimeProvider time)
        => (_visits, _uow, _validator, _time) = (visits, uow, validator, time);

    public async Task HandleAsync(CancelVisitCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var visit = await _visits.GetAsync(cmd.VisitId, ct);

        var party = cmd.ActorId == visit.VisitorId ? VisitParty.Visitor
            : cmd.ActorId == visit.HostId ? VisitParty.Host
            : throw new AppValidationException("No participas en esta visita.");

        visit.Cancel(party, cmd.Reason, _time.GetUtcNow());

        await _visits.AppendAsync(visit, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
