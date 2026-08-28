using NotificationLog.NotificationService.Domain.Shared;

namespace NotificationLog.NotificationService.Application.Templates.Commands.CreateTemplate;

public sealed record CreateTemplateCommand(
    string Name,
    NotificationChannel Channel,
    string? Subject,
    string Body);