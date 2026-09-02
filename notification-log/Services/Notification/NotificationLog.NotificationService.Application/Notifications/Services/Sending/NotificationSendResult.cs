namespace NotificationLog.NotificationService.Application.Notifications.Services.Sending;

public sealed record NotificationSendResult(bool Succeeded, string? ProviderMessageId, string? Error)
{
    public static NotificationSendResult Success(string? providerMessageId) => new(true, providerMessageId, null);

    public static NotificationSendResult Failure(string error) => new(false, null, error);
}
