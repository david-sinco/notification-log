using Application.Shared.Abstractions;
using Application.Shared.Common;
using NotificationLog.NotificationService.Domain.Triggers;

namespace NotificationLog.NotificationService.Application.Triggers.Commands.EnableConfiguration;

public sealed class EnableConfigurationHandler
{
    private readonly INotificationTriggerRepository _triggers;
    private readonly IUnitOfWork _uow;

    public EnableConfigurationHandler(INotificationTriggerRepository triggers, IUnitOfWork uow)
        => (_triggers, _uow) = (triggers, uow);

    public async Task HandleAsync(EnableConfigurationCommand cmd, CancellationToken ct)
    {
        var trigger = await _triggers.GetByIdAsync(cmd.TriggerId, ct)
            ?? throw new NotFoundException(nameof(NotificationTrigger), cmd.TriggerId);

        trigger.EnableConfiguration(cmd.ConfigurationId);

        await _uow.SaveChangesAsync(ct);
    }
}
