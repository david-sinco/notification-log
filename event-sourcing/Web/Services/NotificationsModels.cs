namespace Web.Services;

public enum ChannelDto { Email, Sms }

public enum StatusDto { Scheduled, Sending, Sent, Failed, Discarded }

public sealed record NotificationDto(
    Guid Id, Guid UserId, ChannelDto Channel, StatusDto Status,
    int Attempts, DateTimeOffset ScheduledFor, DateTimeOffset? SentAt, string? LastError);

public sealed record ReplicaStatusDto(int Count);

public sealed record UserContactDto(
    Guid Id, string Name, string? Email, string? Phone, bool EmailVerified, bool PhoneVerified,
    bool Active, bool EmailOptIn, bool SmsOptIn, TimeOnly? QuietHoursStart, TimeOnly? QuietHoursEnd,
    int Version, DateTimeOffset UpdatedAt);
