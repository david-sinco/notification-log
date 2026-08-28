using Users.Domain;

namespace Users.Application.IntegrationEvents;

/// <summary>
/// Translates at the boundary (SPEC.md §5). Of User's ~10 domain event types, only these five
/// change the *verified* contact snapshot and warrant publishing — roughly the 5:1 ratio the
/// spec calls out. EmailChangeRequested and friends never reach here: a downstream consumer has
/// no business knowing about an unverified address.
/// </summary>
public static class IntegrationEventMapper
{
    public static UserContactUpdated? MapIfChanged(User user, IReadOnlyList<object> newEvents)
    {
        var triggered = newEvents.Any(e => e is EmailVerified or PhoneVerified or PreferencesChanged
            or UserDeactivated or UserReactivated);

        if (!triggered)
            return null;

        return new UserContactUpdated(
            UserId: user.Id,
            Name: user.Name,
            Email: user.VerifiedEmail,
            Phone: user.VerifiedPhone,
            EmailVerified: user.VerifiedEmail is not null,
            PhoneVerified: user.VerifiedPhone is not null,
            Active: user.IsActive,
            Preferences: new ContactPreferences(
                user.Preferences.EmailEnabled,
                user.Preferences.SmsEnabled,
                user.Preferences.QuietHoursStart,
                user.Preferences.QuietHoursEnd));
    }
}
