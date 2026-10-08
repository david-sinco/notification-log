namespace NotificationLog.RentalService.Domain.Visitors;

public interface IVisitorRepository
{
    Task<Visitor?> LoadAsync(Guid id, CancellationToken cancellationToken = default);

    Task AppendAsync(Visitor visitor, CancellationToken cancellationToken = default);
}
