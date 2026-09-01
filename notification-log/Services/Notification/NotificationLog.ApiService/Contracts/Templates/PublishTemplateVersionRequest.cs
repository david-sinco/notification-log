namespace NotificationLog.ApiService.Contracts.Templates;

public sealed record PublishTemplateVersionRequest(string? Subject, string Body);
