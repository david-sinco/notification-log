namespace NotificationLog.IdentityService.Domain.Users;

public static class AccountPolicy
{
    public const int PasswordMinLength = 8;
    public const int PasswordMaxLength = 128;
    public const int EmailMaxLength = 256;
    public const int MaxFailedAttempts = 5;
    public const int MaxUsersPerPage = 100;
    public const string DefaultCountryCode = "+57";
    public const int NameMaxLength = 200;
    public const int LocaleMaxLength = 20;
    public const int TimeZoneMaxLength = 50;
    public const string DefaultLocale = "es-CO";
    public const string DefaultTimeZone = "America/Bogota";

    public static readonly IReadOnlyDictionary<string, string> Locales = new Dictionary<string, string>
    {
        ["es-CO"] = "Español (Colombia)",
        ["en-US"] = "English (United States)"
    };

    public static readonly IReadOnlyDictionary<string, string> TimeZones = new Dictionary<string, string>
    {
        ["America/Bogota"] = "Bogotá (GMT-5)",
        ["America/Mexico_City"] = "Ciudad de México (GMT-6)",
        ["America/Lima"] = "Lima (GMT-5)",
        ["America/Santiago"] = "Santiago de Chile",
        ["America/Argentina/Buenos_Aires"] = "Buenos Aires (GMT-3)",
        ["America/New_York"] = "Nueva York",
        ["Europe/Madrid"] = "Madrid"
    };

    public static readonly TimeSpan FailedAttemptsLockout = TimeSpan.FromMinutes(15);

    public static string? ValidatePassword(string? password)
    {
        if (string.IsNullOrEmpty(password))
            return "La contraseña es obligatoria.";

        if (password.Length is < PasswordMinLength or > PasswordMaxLength)
            return $"La contraseña debe tener entre {PasswordMinLength} y {PasswordMaxLength} caracteres.";

        if (!password.Any(char.IsLetter) || !password.Any(char.IsDigit))
            return "La contraseña debe tener al menos una letra y un número.";

        return null;
    }

    public static string? ValidateName(string? name) =>
        string.IsNullOrWhiteSpace(name) || name.Trim().Length > NameMaxLength
            ? $"Escribe tu nombre (máximo {NameMaxLength} caracteres)."
            : null;

    public static string? ValidateLocale(string? locale) =>
        locale is null || !Locales.ContainsKey(locale)
            ? "Elige un idioma válido."
            : null;

    public static string? ValidateTimeZone(string? timeZone) =>
        timeZone is null || !TimeZones.ContainsKey(timeZone)
            ? "Elige una zona horaria válida."
            : null;

    public static string? ValidateConsent(bool acceptsNotifications) =>
        acceptsNotifications
            ? null
            : "Debes autorizar el envío de notificaciones para crear tu cuenta.";
}
