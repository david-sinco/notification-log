namespace NotificationLog.NotificationService.Application.Abstractions;

public interface IUserChangeStream
{
    IAsyncEnumerable<UserChange> ReadAsync(CancellationToken ct);
}