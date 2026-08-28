using NotificationLog.NotificationService.Domain.Shared;

namespace NotificationLog.NotificationService.Application.Templates.Queries.ListTemplates;

public sealed record ListTemplatesQuery(
    string? Search = null,
    NotificationChannel? Channel = null,
    bool? IsEnabled = null,
    int Page = 1,
    int PageSize = 20);