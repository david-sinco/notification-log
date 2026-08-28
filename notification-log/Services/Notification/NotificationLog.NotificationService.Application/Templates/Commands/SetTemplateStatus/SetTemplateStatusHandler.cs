using NotificationLog.NotificationService.Application.Abstractions;
using NotificationLog.NotificationService.Application.Common;
using NotificationLog.NotificationService.Domain.Templates;

namespace NotificationLog.NotificationService.Application.Templates.Commands.SetTemplateStatus;

public sealed class SetTemplateStatusHandler
{
    private readonly INotificationTemplateRepository _templates;
    private readonly IUnitOfWork _uow;

    public async Task HandleAsync(SetTemplateStatusCommand cmd, CancellationToken ct)
    {
        var template = await _templates.GetByIdAsync(cmd.Id, ct)
            ?? throw new NotFoundException(nameof(NotificationTemplate), cmd.Id);

        if (cmd.IsEnabled)
            template.Enable();
        else
            template.Disable();

        await _uow.SaveChangesAsync(ct);
    }
}