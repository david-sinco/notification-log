using Domain.Shared.Common;
using Domain.Shared.Exceptions;
using NotificationLog.IdentityService.Domain.Users.Enums;
using NotificationLog.IdentityService.Domain.Users.ValueObjects;

namespace NotificationLog.IdentityService.Domain.Users;

public sealed class User : AggregateRoot
{
    private readonly List<string> _roles = [];

    private User(Guid id) : base(id) { }

    public string Name { get; private set; } = string.Empty;

    public string? Email { get; private set; }

    public bool IsEmailConfirmed { get; private set; }

    public string? Phone { get; private set; }

    public bool IsPhoneConfirmed { get; private set; }

    public string Locale { get; private set; } = AccountPolicy.DefaultLocale;

    public string TimeZone { get; private set; } = AccountPolicy.DefaultTimeZone;

    public bool AcceptsNotifications { get; private set; }

    public string SecurityStamp { get; private set; } = string.Empty;

    public DateTimeOffset? LockedUntil { get; private set; }

    public IReadOnlyList<string> Roles => _roles;

    public bool IsVerified => IsEmailConfirmed || IsPhoneConfirmed;

    public string DisplayName => string.IsNullOrWhiteSpace(Name) ? Email ?? Phone ?? Id.ToString() : Name;

    public string VerifiedEmail => IsEmailConfirmed ? Email ?? string.Empty : string.Empty;

    public string VerifiedPhone => IsPhoneConfirmed ? Phone ?? string.Empty : string.Empty;

    public static User Register(
        Guid id,
        LoginIdentifier login,
        string name,
        string locale,
        string timeZone,
        bool acceptsNotifications)
    {
        if (id == Guid.Empty)
            throw new DomainException("La cuenta necesita un identificador.");

        ArgumentNullException.ThrowIfNull(login);

        var user = new User(id);

        user.SetContact(login);
        user.UpdateProfile(name, locale, timeZone, acceptsNotifications);

        return user;
    }

    public static User Restore(
        Guid id,
        string name,
        string? email,
        bool isEmailConfirmed,
        string? phone,
        bool isPhoneConfirmed,
        string locale,
        string timeZone,
        bool acceptsNotifications,
        string securityStamp,
        DateTimeOffset? lockedUntil,
        IEnumerable<string> roles)
    {
        var user = new User(id)
        {
            Name = name,
            Email = email,
            IsEmailConfirmed = isEmailConfirmed,
            Phone = phone,
            IsPhoneConfirmed = isPhoneConfirmed,
            Locale = locale,
            TimeZone = timeZone,
            AcceptsNotifications = acceptsNotifications,
            SecurityStamp = securityStamp,
            LockedUntil = lockedUntil
        };

        user._roles.AddRange(roles);

        return user;
    }

    public void UpdateProfile(string name, string locale, string timeZone, bool acceptsNotifications)
    {
        Ensure(AccountPolicy.ValidateName(name));
        Ensure(AccountPolicy.ValidateLocale(locale));
        Ensure(AccountPolicy.ValidateTimeZone(timeZone));
        Ensure(AccountPolicy.ValidateConsent(acceptsNotifications));

        Name = name.Trim();
        Locale = locale;
        TimeZone = timeZone;
        AcceptsNotifications = acceptsNotifications;
    }

    public void SetContact(LoginIdentifier contact)
    {
        ArgumentNullException.ThrowIfNull(contact);

        if (contact.Channel == LoginChannel.Email)
        {
            if (Email == contact.Value)
                return;

            Email = contact.Value;
            IsEmailConfirmed = false;
        }
        else
        {
            if (Phone == contact.Value)
                return;

            Phone = contact.Value;
            IsPhoneConfirmed = false;
        }
    }

    public bool IsConfirmed(LoginChannel channel) =>
        channel == LoginChannel.Email ? IsEmailConfirmed : IsPhoneConfirmed;

    public void Confirm(LoginChannel channel)
    {
        if (channel == LoginChannel.Email)
        {
            if (string.IsNullOrEmpty(Email))
                throw new DomainException("La cuenta no tiene un correo que confirmar.");

            IsEmailConfirmed = true;
        }
        else
        {
            if (string.IsNullOrEmpty(Phone))
                throw new DomainException("La cuenta no tiene un teléfono que confirmar.");

            IsPhoneConfirmed = true;
        }
    }

    public bool IsLockedOut(DateTimeOffset now) => LockedUntil > now;

    private static void Ensure(string? error)
    {
        if (error is not null)
            throw new DomainException(error);
    }
}
