namespace NotificationLog.IdentityService.Api.Accounts.Verification;

public sealed class SmtpOptions
{
    public const string SectionName = "Verification:Email";

    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 587;
    public string From { get; init; } = string.Empty;
    public bool EnableSsl { get; init; } = true;
    public string? Username { get; init; }
    public string? Password { get; init; }
}
