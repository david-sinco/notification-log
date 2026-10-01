namespace NotificationLog.RentalService.Domain.Visits;

public interface IVisitRepository
{
    Task<Visit?> LoadAsync(Guid id, CancellationToken cancellationToken = default);

    Task AppendAsync(Visit visit, CancellationToken cancellationToken = default);
}
