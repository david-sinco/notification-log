using Application.Shared.Abstractions;
using Application.Shared.Common;
using FluentValidation;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Visits;

namespace NotificationLog.RentalService.Application.Visits.Commands.DeclineVisit;

public sealed class DeclineVisitHandler
{
    private readonly IVisitRepository _visits;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<DeclineVisitCommand> _validator;

    public DeclineVisitHandler(IVisitRepository visits, IUnitOfWork uow, IValidator<DeclineVisitCommand> validator)
        => (_visits, _uow, _validator) = (visits, uow, validator);

    public async Task HandleAsync(DeclineVisitCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var visit = await _visits.GetAsync(cmd.VisitId, ct);

        if (visit.HostId != cmd.ActorId)
            throw new AppValidationException("Solo el anfitrión puede rechazar la visita.");

        visit.Decline(cmd.Reason);

        await _visits.AppendAsync(visit, ct);
        await _uow.SaveChangesAsync(ct);
    }
}
