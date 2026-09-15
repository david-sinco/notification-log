namespace NotificationLog.RentalService.Application.Abstractions;

public interface ICommandScheduler
{
    Task ScheduleAsync(object command, DateTimeOffset at, CancellationToken ct);
}
