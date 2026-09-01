using NotificationLog.NotificationService.Application.Common;
using NotificationLog.NotificationService.Application.Recipients.Dtos;
using NotificationLog.NotificationService.Domain.Recipients;

namespace NotificationLog.NotificationService.Application.Recipients.Queries.ListRecipients;

public sealed class ListRecipientsHandler
{
    private readonly IRecipientRepository _recipients;

    public ListRecipientsHandler(IRecipientRepository recipients)
        => _recipients = recipients;

    public async Task<PagedResult<RecipientSummaryDto>> HandleAsync(
        ListRecipientsQuery query, CancellationToken ct)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = Math.Clamp(query.PageSize, 1, 100);

        var (items, total) = await _recipients.ListAsync(
            query.Search, query.IsActive, page, pageSize, ct);

        var dtos = items
            .Select(r => new RecipientSummaryDto(
                r.Id, r.Name, r.Email, r.Phone, r.IsActive))
            .ToList();

        return new PagedResult<RecipientSummaryDto>(dtos, page, pageSize, total);
    }
}