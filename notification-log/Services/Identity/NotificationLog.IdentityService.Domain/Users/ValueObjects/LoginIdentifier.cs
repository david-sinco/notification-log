using System.Text.RegularExpressions;
using NotificationLog.IdentityService.Domain.Users.Enums;

namespace NotificationLog.IdentityService.Domain.Users.ValueObjects;

public sealed record LoginIdentifier(LoginChannel Channel, string Value)
{
    private static readonly Regex EmailFormat =
        new(@"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex PhoneFormat =
        new(@"^\+[1-9]\d{7,14}$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static LoginIdentifier? TryParse(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return null;

        var value = input.Trim();

        if (value.Contains('@'))
        {
            var email = value.ToLowerInvariant();

            return email.Length <= AccountPolicy.EmailMaxLength && EmailFormat.IsMatch(email)
                ? new LoginIdentifier(LoginChannel.Email, email)
                : null;
        }

        if (value.Any(char.IsLetter))
            return null;

        var phone = new string([.. value.Where(c => char.IsAsciiDigit(c) || c == '+')]);

        if (phone.StartsWith("00", StringComparison.Ordinal))
            phone = string.Concat("+", phone.AsSpan(2));
        else if (!phone.StartsWith('+'))
            phone = AccountPolicy.DefaultCountryCode + phone;

        return PhoneFormat.IsMatch(phone) ? new LoginIdentifier(LoginChannel.Phone, phone) : null;
    }
}
