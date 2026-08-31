using NotificationLog.NotificationService.Domain.Shared;

namespace NotificationLog.ApiService.Contracts.Triggers;

public sealed record AddConfigurationRequest(Guid TemplateId, NotificationChannel Channel);
