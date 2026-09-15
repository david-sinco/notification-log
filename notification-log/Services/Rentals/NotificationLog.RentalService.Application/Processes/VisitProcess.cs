using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Application.Visits.Commands.AutoCompleteVisit;
using NotificationLog.RentalService.Application.Visits.Commands.ExpireVisitRequest;
using NotificationLog.RentalService.Application.Visits.Commands.SendVisitReminder;
using NotificationLog.RentalService.Domain.Visits;

namespace NotificationLog.RentalService.Application.Processes;

public sealed class VisitProcess
{
    private static readonly TimeSpan[] ReminderLeadTimes = [TimeSpan.FromHours(24), TimeSpan.FromHours(2)];

    private readonly ICommandScheduler _scheduler;
    private readonly TimeProvider _time;

    public VisitProcess(ICommandScheduler scheduler, TimeProvider time)
        => (_scheduler, _time) = (scheduler, time);

    public Task OnRequestedAsync(Guid visitId, DateTimeOffset respondBy, CancellationToken ct)
        => _scheduler.ScheduleAsync(new ExpireVisitRequestCommand(visitId), respondBy, ct);

    public async Task OnConfirmedAsync(Guid visitId, DateTimeOffset slotStart, CancellationToken ct)
    {
        var now = _time.GetUtcNow();

        foreach (var leadTime in ReminderLeadTimes)
        {
            if (slotStart - leadTime > now)
                await _scheduler.ScheduleAsync(new SendVisitReminderCommand(visitId, slotStart), slotStart - leadTime, ct);
        }

        await _scheduler.ScheduleAsync(
            new AutoCompleteVisitCommand(visitId),
            slotStart + VisitPolicy.SlotDuration + VisitPolicy.AutoCompleteAfter,
            ct);
    }
}
