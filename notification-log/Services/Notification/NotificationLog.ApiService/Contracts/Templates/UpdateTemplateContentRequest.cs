namespace NotificationLog.ApiService.Contracts.Templates;

public sealed record UpdateTemplateContentRequest(string? Subject, string Body);