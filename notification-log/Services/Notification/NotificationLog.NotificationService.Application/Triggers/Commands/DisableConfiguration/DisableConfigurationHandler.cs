using NotificationLog.NotificationService.Application.Abstractions;
using NotificationLog.NotificationService.Application.Common;
using NotificationLog.NotificationService.Domain.Triggers;

namespace NotificationLog.NotificationService.Application.Triggers.Commands.DisableConfiguration;

public sealed class DisableConfigurationHandler
{
    private readonly INotificationTriggerRepository _triggers;
    private readonly IUnitOfWork _uow;

    public DisableConfigurationHandler(INotificationTriggerRepository triggers, IUnitOfWork uow)
        => (_triggers, _uow) = (triggers, uow);

    public async Task HandleAsync(DisableConfigurationCommand cmd, CancellationToken ct)
    {
        var trigger = await _triggers.GetByIdAsync(cmd.TriggerId, ct)
            ?? throw new NotFoundException(nameof(NotificationTrigger), cmd.TriggerId);

        trigger.DisableConfiguration(cmd.ConfigurationId);

        await _uow.SaveChangesAsync(ct);
    }
}
