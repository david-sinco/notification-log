using NotificationLog.NotificationService.Domain.Shared;

namespace NotificationLog.ApiService.Contracts.Templates;

public sealed record CreateTemplateRequest(
    string Name,
    NotificationChannel Channel,
    string? Subject,
    string Body);