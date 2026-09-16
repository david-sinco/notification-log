namespace NotificationLog.IdentityService.Api.Accounts;

public static class AccountPolicy
{
    public const int PasswordMinLength = 8;
    public const int PasswordMaxLength = 128;
    public const int EmailMaxLength = 256;
    public const int MaxFailedAttempts = 5;
    public const int MaxUsersPerPage = 100;
    public const string DefaultCountryCode = "+57";

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
}
