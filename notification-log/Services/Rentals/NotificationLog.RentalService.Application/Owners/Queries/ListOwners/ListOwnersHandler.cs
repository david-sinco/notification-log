using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Application.Owners.Dtos;

namespace NotificationLog.RentalService.Application.Owners.Queries.ListOwners;

public sealed class ListOwnersHandler
{
    private readonly IOwnerReadModel _owners;

    public ListOwnersHandler(IOwnerReadModel owners) => _owners = owners;

    public async Task<PagedResult<OwnerDto>> HandleAsync(ListOwnersQuery query, CancellationToken ct)
    {
        var paged = query with { Page = Paging.Page(query.Page), PageSize = Paging.Size(query.PageSize) };
        var (items, total) = await _owners.ListAsync(paged, ct);

        return new PagedResult<OwnerDto>(items, paged.Page, paged.PageSize, total);
    }
}
