using FluentValidation;
using Application.Shared.Abstractions;
using Application.Shared.Common;
using NotificationLog.NotificationService.Domain.Notifications;

namespace NotificationLog.NotificationService.Application.Notifications.Commands.RecordNotificationSuccess;

public sealed class RecordNotificationSuccessHandler
{
    private readonly INotificationRepository _notifications;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<RecordNotificationSuccessCommand> _validator;

    public RecordNotificationSuccessHandler(
        INotificationRepository notifications,
        IUnitOfWork uow,
        IValidator<RecordNotificationSuccessCommand> validator)
        => (_notifications, _uow, _validator) = (notifications, uow, validator);

    public async Task<Guid> HandleAsync(RecordNotificationSuccessCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var notification = Notification.RecordSuccess(
            cmd.EventId,
            cmd.EventKey,
            cmd.ConfigurationId,
            cmd.TemplateId,
            cmd.TemplateVersionId,
            cmd.RecipientId,
            cmd.Channel,
            cmd.Destination,
            cmd.ProviderMessageId,
            cmd.OccurredAt,
            cmd.Payload);

        await _notifications.AddAsync(notification, ct);
        await _uow.SaveChangesAsync(ct);

        return notification.Id;
    }
}
