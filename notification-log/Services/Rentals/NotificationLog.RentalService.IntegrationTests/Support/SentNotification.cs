namespace NotificationLog.RentalService.IntegrationTests.Support;

public sealed record SentNotification(string EventKey, Guid RecipientId, IReadOnlyDictionary<string, string> Data);
