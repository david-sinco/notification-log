using Domain.Shared.Common;
using Domain.Shared.Exceptions;
using NotificationLog.NotificationService.Domain.Shared;

namespace NotificationLog.NotificationService.Domain.Templates;

public sealed class NotificationTemplate : AggregateRoot
{
    private readonly List<TemplateVersion> _versions = new();

    public TemplateName Name { get; private set; } = default!;
    public NotificationChannel Channel { get; private set; }
    public bool IsEnabled { get; private set; }
    public bool IsSystem { get; private set; }

    public IReadOnlyCollection<TemplateVersion> Versions => _versions.AsReadOnly();

    public TemplateVersion CurrentVersion
        => _versions.SingleOrDefault(v => v.IsCurrent)
           ?? throw new DomainException("La plantilla no tiene una versión vigente.");

    private NotificationTemplate() { }   // EF Core

    public static NotificationTemplate Create(
        TemplateName name, NotificationChannel channel, string? subject, string body)
    {
        ArgumentNullException.ThrowIfNull(name);

        if (!Enum.IsDefined(channel))
            throw new DomainException($"El canal '{channel}' no es válido.");

        var template = new NotificationTemplate
        {
            Id = Guid.NewGuid(),
            Name = name,
            Channel = channel,
            IsEnabled = true,
            IsSystem = false
        };

        template._versions.Add(TemplateVersion.Create(1, subject, body, channel));

        return template;
    }

    public Guid PublishVersion(string? subject, string body)
    {
        var current = CurrentVersion;
        var next = TemplateVersion.Create(current.Number + 1, subject, body, Channel);

        current.Retire();
        _versions.Add(next);

        return next.Id;
    }

    public TemplateVersion? VersionById(Guid versionId)
        => _versions.SingleOrDefault(v => v.Id == versionId);

    public void Enable() => IsEnabled = true;

    public void Disable()
    {
        if (IsSystem)
            throw new DomainException(
                $"La plantilla '{Name.Value}' es del sistema y no se puede deshabilitar.");

        IsEnabled = false;
    }
}