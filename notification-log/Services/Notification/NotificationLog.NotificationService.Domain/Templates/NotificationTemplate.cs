using NotificationLog.NotificationService.Domain.Common;
using NotificationLog.NotificationService.Domain.Shared;

namespace NotificationLog.NotificationService.Domain.Templates;

public sealed class NotificationTemplate : AggregateRoot
{
    public const int SubjectMaxLength = 200;
    public const int BodyMaxLength = 20_000;

    public TemplateName Name { get; private set; } = default!;
    public NotificationChannel Channel { get; private set; }
    public string? Subject { get; private set; }
    public string Body { get; private set; } = default!;
    public bool IsEnabled { get; private set; }

    private NotificationTemplate() { }   // EF Core

    public static NotificationTemplate Create(TemplateName name, NotificationChannel channel, string? subject, string body)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (!Enum.IsDefined(channel))
            throw new DomainException($"El canal '{channel}' no es válido.");

        return new NotificationTemplate
        {
            Id = Guid.NewGuid(),
            Name = name,
            Channel = channel,
            Subject = NormalizeSubject(subject, channel),
            Body = NormalizeBody(body),
            IsEnabled = true
        };
    }

    public void UpdateContent(string? subject, string body)
    {
        Subject = NormalizeSubject(subject, Channel);
        Body = NormalizeBody(body);
    }

    public void Enable() => IsEnabled = true;

    public void Disable() => IsEnabled = false;

    private static string? NormalizeSubject(string? subject, NotificationChannel channel)
    {
        var requiresSubject = channel is NotificationChannel.Email or NotificationChannel.Push;

        if (!requiresSubject)
        {
            if (!string.IsNullOrWhiteSpace(subject))
                throw new DomainException(
                    $"Las plantillas de {channel} no admiten asunto.");

            return null;
        }

        if (string.IsNullOrWhiteSpace(subject))
            throw new DomainException($"Las plantillas de {channel} requieren asunto.");

        var trimmed = subject.Trim();

        if (trimmed.Length > SubjectMaxLength)
            throw new DomainException($"El asunto no puede superar {SubjectMaxLength} caracteres.");

        return trimmed;
    }

    private static string NormalizeBody(string body)
    {
        if (string.IsNullOrWhiteSpace(body))
            throw new DomainException("El cuerpo de la plantilla es obligatorio.");

        var trimmed = body.Trim();

        if (trimmed.Length > BodyMaxLength)
            throw new DomainException($"El cuerpo no puede superar {BodyMaxLength} caracteres.");

        return trimmed;
    }
}