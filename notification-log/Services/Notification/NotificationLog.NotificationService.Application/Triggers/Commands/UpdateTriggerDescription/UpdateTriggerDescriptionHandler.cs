using FluentValidation;
using NotificationLog.NotificationService.Application.Abstractions;
using NotificationLog.NotificationService.Application.Common;
using NotificationLog.NotificationService.Domain.Triggers;

namespace NotificationLog.NotificationService.Application.Triggers.Commands.UpdateTriggerDescription;

public sealed class UpdateTriggerDescriptionHandler
{
    private readonly INotificationTriggerRepository _triggers;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<UpdateTriggerDescriptionCommand> _validator;

    public UpdateTriggerDescriptionHandler(
        INotificationTriggerRepository triggers,
        IUnitOfWork uow,
        IValidator<UpdateTriggerDescriptionCommand> validator)
        => (_triggers, _uow, _validator) = (triggers, uow, validator);

    public async Task HandleAsync(UpdateTriggerDescriptionCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var trigger = await _triggers.GetByIdAsync(cmd.Id, ct)
            ?? throw new NotFoundException(nameof(NotificationTrigger), cmd.Id);

        trigger.UpdateDescription(cmd.Description);

        await _uow.SaveChangesAsync(ct);
    }
}
