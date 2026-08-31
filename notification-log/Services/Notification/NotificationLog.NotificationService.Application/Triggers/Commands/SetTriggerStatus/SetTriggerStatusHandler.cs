using NotificationLog.NotificationService.Application.Abstractions;
using NotificationLog.NotificationService.Application.Common;
using NotificationLog.NotificationService.Domain.Triggers;

namespace NotificationLog.NotificationService.Application.Triggers.Commands.SetTriggerStatus;

public sealed class SetTriggerStatusHandler
{
    private readonly INotificationTriggerRepository _triggers;
    private readonly IUnitOfWork _uow;

    public SetTriggerStatusHandler(INotificationTriggerRepository triggers, IUnitOfWork uow)
        => (_triggers, _uow) = (triggers, uow);

    public async Task HandleAsync(SetTriggerStatusCommand cmd, CancellationToken ct)
    {
        var trigger = await _triggers.GetByIdAsync(cmd.Id, ct)
            ?? throw new NotFoundException(nameof(NotificationTrigger), cmd.Id);

        if (cmd.IsEnabled)
            trigger.Enable();
        else
            trigger.Disable();

        await _uow.SaveChangesAsync(ct);
    }
}
