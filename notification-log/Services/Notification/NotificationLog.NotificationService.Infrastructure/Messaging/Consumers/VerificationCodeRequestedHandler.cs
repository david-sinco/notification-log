using NotificationLog.Contracts.Identity;
using NotificationLog.NotificationService.Application.Notifications.Commands.SendVerificationCode;
using NotificationLog.NotificationService.Domain.Shared;

namespace NotificationLog.NotificationService.Infrastructure.Messaging.Consumers;

public static class VerificationCodeRequestedHandler
{
    public static Task Handle(
        VerificationCodeRequested message, SendVerificationCodeHandler handler, CancellationToken ct)
        => handler.HandleAsync(
            new SendVerificationCodeCommand(
                EventId: Guid.Parse(message.EventId),
                Channel: MapChannel(message.Channel),
                Destination: message.Address,
                Code: message.Code),
            ct);

    private static NotificationChannel MapChannel(VerificationChannel channel) => channel switch
    {
        VerificationChannel.Email => NotificationChannel.Email,
        VerificationChannel.Sms => NotificationChannel.Sms,
        _ => throw new InvalidOperationException($"El canal de verificación '{channel}' no es válido.")
    };
}
