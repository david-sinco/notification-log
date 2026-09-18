namespace NotificationLog.Web.Api.Templates;

public sealed record TemplateSummaryDto(Guid Id, string Name, string Channel, int CurrentVersion, bool IsEnabled);

public sealed record TemplateDto(
    Guid Id,
    string Name,
    string Channel,
    bool IsEnabled,
    TemplateVersionDto CurrentVersion,
    IReadOnlyList<TemplateVersionDto> History);

public sealed record TemplateVersionDto(Guid Id, int Number, string? Subject, string Body, bool IsCurrent, DateTime CreatedAt);

public sealed record CreateTemplateRequest(string Name, NotificationChannel Channel, string? Subject, string Body);

public sealed record PublishTemplateVersionRequest(string? Subject, string Body);

public sealed record SetTemplateStatusRequest(bool IsEnabled);

// Espejo de NotificationLog.NotificationService.Domain.Templates.TemplateVersion.
public static class TemplateLimits
{
    public const int SubjectMaxLength = 200;
    public const int BodyMaxLength = 20_000;

    public static bool RequiresSubject(NotificationChannel channel) =>
        channel is NotificationChannel.Email or NotificationChannel.Push;
}
