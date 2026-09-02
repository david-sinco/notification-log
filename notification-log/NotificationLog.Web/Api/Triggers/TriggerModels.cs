namespace NotificationLog.Web.Api.Triggers;

public sealed record TriggerSummaryDto(
    Guid Id, string EventKey, string Description, bool IsEnabled, int ActiveConfigurations);

public sealed record TriggerDto(
    Guid Id, string EventKey, string Description, bool IsEnabled, IReadOnlyList<ConfigurationDto> Configurations);

public sealed record ConfigurationDto(Guid Id, Guid TemplateId, string Channel, bool IsEnabled);

public sealed record CreateTriggerRequest(string EventKey, string Description);

public sealed record UpdateTriggerDescriptionRequest(string Description);

public sealed record SetTriggerStatusRequest(bool IsEnabled);

public sealed record AddConfigurationRequest(Guid TemplateId, NotificationChannel Channel);

public sealed record ChangeConfigurationTemplateRequest(Guid TemplateId);
