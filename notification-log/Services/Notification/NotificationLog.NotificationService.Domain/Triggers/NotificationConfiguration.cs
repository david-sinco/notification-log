using NotificationLog.NotificationService.Domain.Common;
using NotificationLog.NotificationService.Domain.Shared;

namespace NotificationLog.NotificationService.Domain.Triggers;

public class NotificationConfiguration: Entity
{
    public Guid TemplateId { get; private set; }
    public NotificationChannel Channel { get; private set; }
    public bool IsEnabled { get; private set; }

    protected NotificationConfiguration() { }


    internal static NotificationConfiguration Create(
        Guid templateId, NotificationChannel channel)
    {
        if (templateId == Guid.Empty)
            throw new DomainException("La configuración requiere una plantilla.");

        if (!Enum.IsDefined(channel))
            throw new DomainException($"El canal '{channel}' no es válido.");

        return new NotificationConfiguration
        {
            Id = Guid.NewGuid(),
            TemplateId = templateId,
            Channel = channel,
            IsEnabled = true
        };
    }

    internal void Enable() => IsEnabled = true;

    internal void Disable() => IsEnabled = false;

    internal void ChangeTemplate(Guid templateId)
    {
        if (templateId == Guid.Empty)
            throw new DomainException("La configuración requiere una plantilla.");

        TemplateId = templateId;
    }
}
