using NotificationLog.RentalService.Application.Owners.Dtos;
using NotificationLog.RentalService.Application.Owners.Queries.ListOwners;
using NotificationLog.RentalService.Domain.Owners.Enums;

namespace NotificationLog.RentalService.Application.Abstractions;

public interface IOwnerReadModel
{
    Task<(IReadOnlyList<OwnerDto> Items, int TotalCount)> ListAsync(ListOwnersQuery query, CancellationToken ct);

    Task<OwnerDto?> GetAsync(Guid id, CancellationToken ct);

    Task<bool> ExistsWithDocumentAsync(DocumentType type, string number, CancellationToken ct);

    Task<bool> ExistsWithNitAsync(string nit, CancellationToken ct);
}
