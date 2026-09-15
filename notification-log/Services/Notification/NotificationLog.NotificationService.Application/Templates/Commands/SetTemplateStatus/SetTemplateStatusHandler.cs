using Application.Shared.Abstractions;
using Application.Shared.Common;
using NotificationLog.NotificationService.Domain.Templates;

namespace NotificationLog.NotificationService.Application.Templates.Commands.SetTemplateStatus;

public sealed class SetTemplateStatusHandler(INotificationTemplateRepository templates, IUnitOfWork uow)
{
    private readonly INotificationTemplateRepository _templates = templates;
    private readonly IUnitOfWork _uow = uow;

    public async Task HandleAsync(SetTemplateStatusCommand cmd, CancellationToken ct)
    {
        var template = await _templates.GetByIdAsync(cmd.TemplateId, ct)
            ?? throw new NotFoundException(nameof(NotificationTemplate), cmd.TemplateId);

        if (cmd.IsEnabled) template.Enable();
        else template.Disable();

        await _uow.SaveChangesAsync(ct);
    }
}
