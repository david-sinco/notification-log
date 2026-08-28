using FluentValidation;
using NotificationLog.NotificationService.Application.Abstractions;
using NotificationLog.NotificationService.Application.Common;
using NotificationLog.NotificationService.Domain.Templates;

namespace NotificationLog.NotificationService.Application.Templates.Commands.UpdateTemplateContent;

public sealed class UpdateTemplateContentHandler
{
    private readonly INotificationTemplateRepository _templates;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<UpdateTemplateContentCommand> _validator;

    public async Task HandleAsync(UpdateTemplateContentCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var template = await _templates.GetByIdAsync(cmd.Id, ct)
            ?? throw new NotFoundException(nameof(NotificationTemplate), cmd.Id);

        template.UpdateContent(cmd.Subject, cmd.Body);

        await _uow.SaveChangesAsync(ct);
    }
}