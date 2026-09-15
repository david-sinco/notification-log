using FluentValidation;
using Application.Shared.Abstractions;
using Application.Shared.Common;
using NotificationLog.NotificationService.Domain.Triggers;
using System.ComponentModel.DataAnnotations;

namespace NotificationLog.NotificationService.Application.Triggers.Commands.CreateTrigger;

public sealed class CreateTriggerHandler
{
    private readonly INotificationTriggerRepository _triggers;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<CreateTriggerCommand> _validator;

    public CreateTriggerHandler(
        INotificationTriggerRepository triggers,
        IUnitOfWork uow,
        IValidator<CreateTriggerCommand> validator)
        => (_triggers, _uow, _validator) = (triggers, uow, validator);

    public async Task<Guid> HandleAsync(CreateTriggerCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var eventKey = EventKey.Create(cmd.EventKey);

        if (await _triggers.ExistsAsync(eventKey, ct))
            throw new AppValidationException($"Ya existe un trigger para '{eventKey}'.");

        var trigger = NotificationTrigger.Create(eventKey, cmd.Description);

        await _triggers.AddAsync(trigger, ct);
        await _uow.SaveChangesAsync(ct);

        return trigger.Id;
    }
}