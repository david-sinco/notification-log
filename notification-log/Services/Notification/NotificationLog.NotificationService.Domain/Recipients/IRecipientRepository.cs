namespace NotificationLog.NotificationService.Domain.Recipients;

public interface IRecipientRepository
{
    Task<Recipient?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<(IReadOnlyList<Recipient> Items, int TotalCount)> ListAsync(
        string? search, bool? isActive, int page, int pageSize, CancellationToken ct);
    Task AddAsync(Recipient recipient, CancellationToken ct);
}