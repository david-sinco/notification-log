namespace NotificationLog.NotificationService.Application.Recipients.Queries.ListRecipients;

public sealed record ListRecipientsQuery(
    string? Search = null,
    bool? IsActive = null,
    int Page = 1,
    int PageSize = 20);