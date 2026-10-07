using Application.Shared.Pagination;
using NotificationLog.RentalService.Application.Owners.Queries.Dtos;
using NotificationLog.RentalService.Application.Owners.Queries.Filters;

namespace NotificationLog.RentalService.Application.Owners.Queries;

public interface IOwnerReadModel
{
    Task<(IReadOnlyList<OwnerDto> Items, int TotalCount)> ListAsync(OwnerFilter filter, PageRequest paging, CancellationToken ct);

    Task<OwnerDto?> GetAsync(Guid id, CancellationToken ct);

    Task<IReadOnlyList<OwnerDto>> ListRelatedToAsync(Guid userId, CancellationToken ct);

    Task<IReadOnlyList<OwnerDto>> ListClaimableAsync(string? email, string? phone, CancellationToken ct);
}
