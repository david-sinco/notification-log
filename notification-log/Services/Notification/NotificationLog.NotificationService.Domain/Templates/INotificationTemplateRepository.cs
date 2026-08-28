using NotificationLog.NotificationService.Domain.Shared;

namespace NotificationLog.NotificationService.Domain.Templates;

public interface INotificationTemplateRepository
{
    Task<NotificationTemplate?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<bool> ExistsAsync(TemplateName name, CancellationToken ct);
    Task<(IReadOnlyList<NotificationTemplate> Items, int TotalCount)> ListAsync(
        string? search, NotificationChannel? channel, bool? isEnabled,
        int page, int pageSize, CancellationToken ct);
    Task AddAsync(NotificationTemplate template, CancellationToken ct);
}