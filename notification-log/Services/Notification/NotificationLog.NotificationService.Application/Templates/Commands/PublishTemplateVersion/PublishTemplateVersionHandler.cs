using FluentValidation;
using NotificationLog.NotificationService.Application.Abstractions;
using NotificationLog.NotificationService.Application.Common;
using NotificationLog.NotificationService.Domain.Templates;

namespace NotificationLog.NotificationService.Application.Templates.Commands.PublishTemplateVersion;

public sealed class PublishTemplateVersionHandler(INotificationTemplateRepository templates, IUnitOfWork uow, IValidator<PublishTemplateVersionCommand> validator)
{
    private readonly INotificationTemplateRepository _templates = templates;
    private readonly IUnitOfWork _uow = uow;
    private readonly IValidator<PublishTemplateVersionCommand> _validator = validator;


    public async Task<Guid> HandleAsync(PublishTemplateVersionCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var template = await _templates.GetByIdAsync(cmd.TemplateId, ct)
            ?? throw new NotFoundException(nameof(NotificationTemplate), cmd.TemplateId);

        var versionId = template.PublishVersion(cmd.Subject, cmd.Body);

        await _uow.SaveChangesAsync(ct);

        return versionId;
    }
}
