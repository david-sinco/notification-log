using FluentValidation;
using NotificationLog.NotificationService.Domain.Notifications;
using NotificationLog.NotificationService.Domain.Shared;

namespace NotificationLog.NotificationService.Application.Notifications.Commands.SendVerificationCode;

internal sealed class SendVerificationCodeValidator : AbstractValidator<SendVerificationCodeCommand>
{
    public const int CodeMaxLength = 20;

    public SendVerificationCodeValidator()
    {
        RuleFor(x => x.EventId).NotEmpty();
        RuleFor(x => x.Channel)
            .Must(channel => channel is NotificationChannel.Email or NotificationChannel.Sms)
            .WithMessage("Los códigos de verificación solo se envían por Email o SMS.");
        RuleFor(x => x.Destination).NotEmpty().MaximumLength(Notification.DestinationMaxLength);
        RuleFor(x => x.Code).NotEmpty().MaximumLength(CodeMaxLength);
    }
}
