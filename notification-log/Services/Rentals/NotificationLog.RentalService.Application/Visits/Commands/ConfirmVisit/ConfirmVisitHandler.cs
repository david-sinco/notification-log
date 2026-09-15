using Application.Shared.Abstractions;
using Application.Shared.Common;
using FluentValidation;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Visits;
using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Application.Visits.Commands.ConfirmVisit;

public sealed class ConfirmVisitHandler
{
    private readonly IVisitRepository _visits;
    private readonly ISoftRuleChecks _rules;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<ConfirmVisitCommand> _validator;
    private readonly TimeProvider _time;

    public ConfirmVisitHandler(
        IVisitRepository visits,
        ISoftRuleChecks rules,
        IUnitOfWork uow,
        IValidator<ConfirmVisitCommand> validator,
        TimeProvider time)
        => (_visits, _rules, _uow, _validator, _time) = (visits, rules, uow, validator, time);

    public async Task HandleAsync(ConfirmVisitCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var visit = await _visits.GetAsync(cmd.VisitId, ct);

        if (visit.HostId != cmd.ActorId)
            throw new AppValidationException("Solo el anfitrión puede confirmar la visita.");

        var slot = TimeSlot.Create(cmd.SlotStart, cmd.SlotStart + VisitPolicy.SlotDuration);

        if (await _rules.HostHasOverlapAsync(visit.HostId, slot, ct))
            throw new AppValidationException("Ya tienes otra visita confirmada en esa franja.");

        visit.Confirm(cmd.SlotStart, _time.GetUtcNow());

        await _visits.AppendAsync(visit, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
