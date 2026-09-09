using Domain.Shared.Common;
using Domain.Shared.Exceptions;
using NotificationLog.NotificationService.Domain.Shared;

namespace NotificationLog.NotificationService.Domain.Templates;

public sealed class TemplateVersion : Entity
{
    public const int SubjectMaxLength = 200;
    public const int BodyMaxLength = 20_000;

    public int Number { get; private set; }
    public string? Subject { get; private set; }
    public string Body { get; private set; } = default!;
    public bool IsCurrent { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private TemplateVersion() { }   // EF Core

    internal static TemplateVersion Create(
        int number, string? subject, string body, NotificationChannel channel)
        => new()
        {
            Id = Guid.NewGuid(),
            Number = number,
            Subject = NormalizeSubject(subject, channel),
            Body = NormalizeBody(body),
            IsCurrent = true,
            CreatedAt = DateTime.UtcNow
        };

    internal void Retire() => IsCurrent = false;

    private static string? NormalizeSubject(string? subject, NotificationChannel channel)
    {
        var requiresSubject = channel is NotificationChannel.Email or NotificationChannel.Push;

        if (!requiresSubject)
        {
            if (!string.IsNullOrWhiteSpace(subject))
                throw new DomainException($"Las plantillas de {channel} no admiten asunto.");

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