using FluentValidation;
using NotificationLog.NotificationService.Application.Abstractions;
using NotificationLog.NotificationService.Application.Common;
using NotificationLog.NotificationService.Domain.Notifications;

namespace NotificationLog.NotificationService.Application.Notifications.Commands.RecordNotificationFailure;

public sealed class RecordNotificationFailureHandler
{
    private readonly INotificationRepository _notifications;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<RecordNotificationFailureCommand> _validator;

    public RecordNotificationFailureHandler(
        INotificationRepository notifications,
        IUnitOfWork uow,
        IValidator<RecordNotificationFailureCommand> validator)
        => (_notifications, _uow, _validator) = (notifications, uow, validator);

    public async Task<Guid> HandleAsync(RecordNotificationFailureCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var notification = Notification.RecordFailure(
            cmd.EventId,
            cmd.EventKey,
            cmd.ConfigurationId,
            cmd.TemplateId,
            cmd.TemplateVersionId,
            cmd.RecipientId,
            cmd.Channel,
            cmd.Destination,
            cmd.Error,
            cmd.OccurredAt,
            cmd.Payload);

        await _notifications.AddAsync(notification, ct);
        await _uow.SaveChangesAsync(ct);

        return notification.Id;
    }
}
