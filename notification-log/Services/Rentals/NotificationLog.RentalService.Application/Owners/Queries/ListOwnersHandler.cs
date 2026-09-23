using Application.Shared.Pagination;
using NotificationLog.RentalService.Application.Owners.Queries.Dtos;
using NotificationLog.RentalService.Application.Owners.Queries.Filters;

namespace NotificationLog.RentalService.Application.Owners.Queries;

public sealed class ListOwnersHandler(IOwnerReadModel owners)
{
    private readonly IOwnerReadModel _owners = owners;

    public async Task<PagedResult<OwnerDto>> HandleAsync(OwnerFilter filter, PageRequest paging, CancellationToken ct)
    {
        var (items, total) = await _owners.ListAsync(filter, paging, ct);

        return new PagedResult<OwnerDto>(items, paging, total);
    }
}
