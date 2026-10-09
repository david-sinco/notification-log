namespace NotificationLog.NotificationService.Application.Templates.Dtos;

public sealed record TemplateSummaryDto(Guid Id, string Name, string Channel, int CurrentVersion, bool IsEnabled, string? Subject, string Body, DateTime PublishedAt);