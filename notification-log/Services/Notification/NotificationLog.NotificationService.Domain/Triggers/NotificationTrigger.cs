using NotificationLog.NotificationService.Domain.Common;
using NotificationLog.NotificationService.Domain.Shared;

namespace NotificationLog.NotificationService.Domain.Triggers;

public class NotificationTrigger : AggregateRoot
{
    public const int DescriptionMaxLength = 500;


    private readonly List<NotificationConfiguration> _configurations = new();

    public EventKey EventKey { get; set; } = default!;
    public string Description { get; set; } = default!;
    public bool IsEnabled { get; set; }

    public IReadOnlyCollection<NotificationConfiguration> Configurations => _configurations.AsReadOnly();

    protected NotificationTrigger() { }

    public static NotificationTrigger Create(EventKey eventKey, string description)
    {
        ArgumentNullException.ThrowIfNull(eventKey);

        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("La descripción del trigger es obligatoria.");

        return new NotificationTrigger
        {
            Id = Guid.NewGuid(),
            EventKey = eventKey,
            Description = NormalizeDescription(description),
            IsEnabled = true
        };
    }

    public NotificationConfiguration AddConfiguration(Guid templateId, NotificationChannel channel)
    {
        EnsureChannelIsFree(channel);

        var configuration = NotificationConfiguration.Create(templateId, channel);
        _configurations.Add(configuration);

        return configuration;
    }

    public void EnableConfiguration(Guid configurationId)
    {
        var configuration = RequireConfiguration(configurationId);

        if (configuration.IsEnabled) return;

        EnsureChannelIsFree(configuration.Channel, exceptId: configurationId);

        configuration.Enable();
    }

    public void DisableConfiguration(Guid configurationId)
    {
        var configuration = RequireConfiguration(configurationId);

        if (!configuration.IsEnabled) return;

        configuration.Disable();
    }

    public void ChangeConfigurationTemplate(
        Guid configurationId, Guid templateId, NotificationChannel templateChannel)
    {
        var configuration = RequireConfiguration(configurationId);

        if (configuration.Channel != templateChannel)
            throw new DomainException(
                $"La configuración es de canal {configuration.Channel} " +
                $"y la plantilla de {templateChannel}.");

        if (configuration.TemplateId == templateId) return;

        configuration.ChangeTemplate(templateId);
    }

    public IReadOnlyList<NotificationConfiguration> ResolveActiveConfigurations()
        => IsEnabled
            ? _configurations.Where(c => c.IsEnabled).ToList()
            : Array.Empty<NotificationConfiguration>();

    public void Enable()
    {
        if (IsEnabled) return;

        IsEnabled = true;
    }

    public void Disable()
    {
        if (!IsEnabled) return;

        IsEnabled = false;
    }

    public void UpdateDescription(string description)
        => Description = NormalizeDescription(description);

    private static string NormalizeDescription(string description)
    {
        if (string.IsNullOrWhiteSpace(description))
            throw new DomainException("La descripción del trigger es obligatoria.");

        var trimmed = description.Trim();

        if (trimmed.Length > DescriptionMaxLength)
            throw new DomainException(
                $"La descripción no puede superar {DescriptionMaxLength} caracteres.");

        return trimmed;
    }


    private NotificationConfiguration RequireConfiguration(Guid configurationId)
        => _configurations.SingleOrDefault(c => c.Id == configurationId)
           ?? throw new DomainException("La configuración no pertenece a este trigger.");

    private void EnsureChannelIsFree(NotificationChannel channel, Guid? exceptId = null)
    {
        var occupied = _configurations.Any(c =>
            c.IsEnabled &&
            c.Channel == channel &&
            c.Id != exceptId);

        if (occupied)
            throw new DomainException(
                $"Ya existe una configuración activa para el canal {channel}.");
    }
}
