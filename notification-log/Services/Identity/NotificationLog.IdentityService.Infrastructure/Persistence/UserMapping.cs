using NotificationLog.IdentityService.Domain.Users;

namespace NotificationLog.IdentityService.Infrastructure.Persistence;

internal static class UserMapping
{
    public static User ToDomain(ApplicationUser entity, IEnumerable<string> roles) =>
        User.Restore(
            entity.Id,
            entity.Name ?? string.Empty,
            entity.Email,
            entity.EmailConfirmed,
            entity.PhoneNumber,
            entity.PhoneNumberConfirmed,
            entity.Locale ?? AccountPolicy.DefaultLocale,
            entity.TimeZone ?? AccountPolicy.DefaultTimeZone,
            entity.AcceptsNotifications,
            entity.SecurityStamp ?? string.Empty,
            entity.LockoutEnd,
            roles);

    public static ApplicationUser ToEntity(User user) =>
        new(user.Id)
        {
            Email = user.Email,
            EmailConfirmed = user.IsEmailConfirmed,
            PhoneNumber = user.Phone,
            PhoneNumberConfirmed = user.IsPhoneConfirmed,
            Name = user.Name,
            Locale = user.Locale,
            TimeZone = user.TimeZone,
            AcceptsNotifications = user.AcceptsNotifications
        };

    public static void Apply(User user, ApplicationUser entity)
    {
        entity.Name = user.Name;
        entity.Locale = user.Locale;
        entity.TimeZone = user.TimeZone;
        entity.AcceptsNotifications = user.AcceptsNotifications;
        entity.EmailConfirmed = user.IsEmailConfirmed;
        entity.PhoneNumberConfirmed = user.IsPhoneConfirmed;
    }
}
