using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Application.Processes;
using NotificationLog.RentalService.Domain.Visits;
using NotificationLog.RentalService.Domain.Visits.Enums;

namespace NotificationLog.RentalService.Application.Visits.Commands.SendVisitReminder;

public sealed class SendVisitReminderHandler
{
    private readonly IVisitRepository _visits;
    private readonly INotificationDispatcher _notifications;

    public SendVisitReminderHandler(IVisitRepository visits, INotificationDispatcher notifications)
        => (_visits, _notifications) = (visits, notifications);

    public async Task HandleAsync(SendVisitReminderCommand cmd, CancellationToken ct)
    {
        var visit = await _visits.LoadAsync(cmd.VisitId, ct);

        if (visit is null || visit.Status != VisitStatus.Confirmed || visit.ConfirmedSlot?.Start != cmd.SlotStart)
            return;

        var data = new Dictionary<string, string>
        {
            ["visit_id"] = visit.Id.ToString(),
            ["listing_id"] = visit.ListingId.ToString(),
            ["slot_start"] = cmd.SlotStart.ToString("O")
        };

        await _notifications.DispatchAsync(NotificationKeys.VisitReminder, visit.VisitorId, data, ct);
        await _notifications.DispatchAsync(NotificationKeys.VisitReminder, visit.HostId, data, ct);
    }
}
