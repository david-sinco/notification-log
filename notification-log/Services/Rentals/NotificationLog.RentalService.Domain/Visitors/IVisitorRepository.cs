using NotificationLog.RentalService.Domain.Common.ValueObjects;

namespace NotificationLog.RentalService.Domain.Visitors;

public interface IVisitorRepository
{
    Task<Visitor?> LoadAsync(Guid id, CancellationToken cancellationToken = default);

    Task AppendAsync(Visitor visitor, CancellationToken cancellationToken = default);

    Task<bool> TryReserveVisitorAsync(Guid visitorId, IdentityDocument document, CancellationToken cancellationToken = default);
}