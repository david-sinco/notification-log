using FluentValidation;
using NotificationLog.NotificationService.Application.Abstractions;
using NotificationLog.NotificationService.Application.Common;
using NotificationLog.NotificationService.Domain.Templates;

namespace NotificationLog.NotificationService.Application.Templates.Commands.CreateTemplate;

public sealed class CreateTemplateHandler
{
    private readonly INotificationTemplateRepository _templates;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<CreateTemplateCommand> _validator;

    public CreateTemplateHandler(
        INotificationTemplateRepository templates,
        IUnitOfWork uow,
        IValidator<CreateTemplateCommand> validator)
        => (_templates, _uow, _validator) = (templates, uow, validator);

    public async Task<Guid> HandleAsync(CreateTemplateCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var name = TemplateName.Create(cmd.Name);

        if (await _templates.ExistsAsync(name, ct))
            throw new AppValidationException($"Ya existe una plantilla llamada '{name}'.");

        var template = NotificationTemplate.Create(name, cmd.Channel, cmd.Subject, cmd.Body);

        await _templates.AddAsync(template, ct);
        await _uow.SaveChangesAsync(ct);

        return template.Id;
    }
}