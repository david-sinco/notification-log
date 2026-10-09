using Application.Shared.Pagination;
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
        var paging = new PageRequest(query.Page, query.PageSize);

        var (items, total) = await _recipients.ListAsync(
            query.Search, query.IsActive, paging.Page, paging.PageSize, ct);

        var dtos = items
            .Select(r => new RecipientSummaryDto(
                r.Id, r.Name, r.Email, r.Phone, r.IsActive, r.AcceptsNotifications))
            .ToList();

        return new PagedResult<RecipientSummaryDto>(dtos, paging, total);
    }
}