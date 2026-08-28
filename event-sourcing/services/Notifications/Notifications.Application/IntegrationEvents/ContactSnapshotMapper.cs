using Notifications.Domain;

namespace Notifications.Application.IntegrationEvents;

public static class ContactSnapshotMapper
{
    public static UserContact ToSnapshot(Envelope<UserContactUpdated> envelope)
    {
        var data = envelope.Data;
        return new UserContact
        {
            Id = data.UserId,
            Name = data.Name,
            Email = data.Email,
            Phone = data.Phone,
            EmailVerified = data.EmailVerified,
            PhoneVerified = data.PhoneVerified,
            Active = data.Active,
            EmailOptIn = data.Preferences.Email,
            SmsOptIn = data.Preferences.Sms,
            QuietHoursStart = data.Preferences.QuietHoursStart,
            QuietHoursEnd = data.Preferences.QuietHoursEnd,
            Version = envelope.Version,
            UpdatedAt = envelope.OccurredAt,
        };
    }
}
