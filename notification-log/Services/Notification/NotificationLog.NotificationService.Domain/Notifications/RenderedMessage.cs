using NotificationLog.NotificationService.Domain.Common;

namespace NotificationLog.NotificationService.Domain.Notifications;

public sealed record RenderedMessage
{
    public const int SubjectMaxLength = 500;
    public const int BodyMaxLength = 50_000;

    public string? Subject { get; }
    public string Body { get; }

    private RenderedMessage(string? subject, string body)
        => (Subject, Body) = (subject, body);

    public static RenderedMessage Create(string? subject, string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            throw new DomainException("El cuerpo del mensaje renderizado es obligatorio.");

        var normalizedSubject = string.IsNullOrWhiteSpace(subject)
            ? null
            : Truncate(subject.Trim(), SubjectMaxLength);

        return new RenderedMessage(normalizedSubject, Truncate(body.Trim(), BodyMaxLength));
    }

    private static string Truncate(string value, int max)
        => value.Length > max ? value[..max] : value;
}