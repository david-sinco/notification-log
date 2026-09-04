using FluentValidation;
using NotificationLog.NotificationService.Domain.Notifications;

namespace NotificationLog.NotificationService.Application.Notifications.Commands.RecordNotificationFailure;

internal sealed class RecordNotificationFailureValidator : AbstractValidator<RecordNotificationFailureCommand>
{
    public RecordNotificationFailureValidator()
    {
        RuleFor(x => x.EventId).NotEmpty();
        RuleFor(x => x.EventKey).NotEmpty().MaximumLength(Notification.EventKeyMaxLength);
        RuleFor(x => x.ConfigurationId).NotEmpty();
        RuleFor(x => x.TemplateId).NotEmpty();
        RuleFor(x => x.TemplateVersionId).NotEmpty();
        RuleFor(x => x.RecipientId).NotEmpty();
        RuleFor(x => x.Channel).IsInEnum();
        RuleFor(x => x.Destination).NotEmpty().MaximumLength(Notification.DestinationMaxLength);
        RuleFor(x => x.Error).NotEmpty().MaximumLength(Notification.ErrorMaxLength);
    }
}
