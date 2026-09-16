namespace NotificationLog.IdentityService.Api.Accounts.Verification;

public sealed class SmsOptions
{
    public const string SectionName = "Verification:Sms";

    public string BaseUrl { get; init; } = "http://localhost:8000";
    public string Path { get; init; } = "/sms/identity";
    public string From { get; init; } = "NotificationLog";
}
