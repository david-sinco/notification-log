using Notifications.Application.Ports;
using Notifications.Domain;

namespace Notifications.Application;

public sealed class NotificationCommandService(
    IUserContactRepository contacts, INotificationRepository notifications, TimeProvider clock)
{
    public async Task<Notification> RequestNotificationAsync(
        Guid userId, NotificationChannel channel, string idempotencyKey, string body, CancellationToken ct)
    {
        var contact = await contacts.FindAsync(userId, ct) ?? throw new UnknownUserException(userId);

        if (!contact.CanReceive(channel))
            throw new ChannelNotReceivableException(userId, channel);

        var sendAt = contact.NextSendableMoment(clock.GetUtcNow());
        var notification = Notification.Schedule(userId, channel, idempotencyKey, body, sendAt);

        await notifications.SaveAsync(notification, ct);
        return notification;
    }
}
