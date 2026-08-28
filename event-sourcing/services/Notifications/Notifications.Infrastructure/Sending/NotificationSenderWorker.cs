using Marten;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Notifications.Application.Ports;
using Notifications.Domain;

namespace Notifications.Infrastructure.Sending;

/// <summary>
/// Polls due Scheduled notifications and sends them. Re-checks CanReceive at send time, not only
/// at schedule time — preferences may have changed in between (SPEC.md §8).
/// </summary>
public sealed class NotificationSenderWorker(IServiceScopeFactory scopeFactory, TimeProvider clock, ILogger<NotificationSenderWorker> logger)
    : BackgroundService
{
    private static readonly TimeSpan PollInterval = TimeSpan.FromSeconds(1);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await SendDueAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Notification sender loop failed");
            }

            await Task.Delay(PollInterval, stoppingToken);
        }
    }

    private async Task SendDueAsync(CancellationToken ct)
    {
        using var scope = scopeFactory.CreateScope();
        var session = scope.ServiceProvider.GetRequiredService<IDocumentSession>();
        var notifications = scope.ServiceProvider.GetRequiredService<INotificationRepository>();
        var contacts = scope.ServiceProvider.GetRequiredService<IUserContactRepository>();
        var sender = scope.ServiceProvider.GetRequiredService<INotificationSender>();

        var now = clock.GetUtcNow();
        var due = await notifications.FindDueAsync(now, limit: 50, ct);
        if (due.Count == 0)
            return;

        foreach (var notification in due)
        {
            var contact = await contacts.FindAsync(notification.UserId, ct);
            if (contact is null || !contact.CanReceive(notification.Channel))
            {
                notification.MarkFailed(permanent: true, "Recipient can no longer receive on this channel.", now);
                session.Store(notification);
                continue;
            }

            notification.BeginSending();
            session.Store(notification);
            await session.SaveChangesAsync(ct);

            var result = await sender.SendAsync(notification, contact, ct);
            switch (result.Outcome)
            {
                case Application.Ports.SendOutcome.Sent:
                    notification.MarkSent(clock.GetUtcNow());
                    break;
                case Application.Ports.SendOutcome.TransientFailure:
                    notification.MarkFailed(permanent: false, result.Error ?? "transient failure", clock.GetUtcNow());
                    break;
                case Application.Ports.SendOutcome.PermanentFailure:
                    notification.MarkFailed(permanent: true, result.Error ?? "permanent failure", clock.GetUtcNow());
                    break;
            }

            session.Store(notification);
        }

        await session.SaveChangesAsync(ct);
    }
}
