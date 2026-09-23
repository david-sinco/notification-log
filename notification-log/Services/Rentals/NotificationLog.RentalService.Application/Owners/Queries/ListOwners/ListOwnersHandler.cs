using Application.Shared.Pagination;
using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Application.Owners.Dtos;

namespace NotificationLog.RentalService.Application.Owners.Queries.ListOwners;

public sealed class ListOwnersHandler(IOwnerReadModel owners)
{
    private readonly IOwnerReadModel _owners = owners;

    public async Task<PagedResult<OwnerDto>> HandleAsync(ListOwnersQuery query, PageRequest paging, CancellationToken ct)
    {
        var (items, total) = await _owners.ListAsync(query, paging, ct);

        return new PagedResult<OwnerDto>(items, paging, total);
    }
}
