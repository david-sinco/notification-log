using NotificationLog.NotificationService.Domain.Shared;

namespace NotificationLog.NotificationService.Application.Notifications.Commands.SendVerificationCode;

public sealed record SendVerificationCodeCommand(
    Guid EventId,
    NotificationChannel Channel,
    string Destination,
    string Code);
