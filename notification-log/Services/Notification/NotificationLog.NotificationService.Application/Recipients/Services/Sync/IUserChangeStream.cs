namespace NotificationLog.NotificationService.Application.Recipients.Services.Sync;

public interface IUserChangeStream
{
    IAsyncEnumerable<UserChange> ReadAsync(CancellationToken ct);
}
