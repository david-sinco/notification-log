using NotificationLog.NotificationService.Domain.Shared;

namespace NotificationLog.NotificationService.Domain.Templates;

public static class SystemTemplates
{
    public static readonly Guid VerificationCodeEmail =
        new("f1a3c4d2-0000-4000-8000-000000000001");

    public static readonly Guid VerificationCodeSms =
        new("f1a3c4d2-0000-4000-8000-000000000002");

    public static Guid? VerificationCodeFor(NotificationChannel channel) => channel switch
    {
        NotificationChannel.Email => VerificationCodeEmail,
        NotificationChannel.Sms => VerificationCodeSms,
        _ => null
    };

    public static bool Contains(Guid templateId)
        => templateId == VerificationCodeEmail || templateId == VerificationCodeSms;
}
