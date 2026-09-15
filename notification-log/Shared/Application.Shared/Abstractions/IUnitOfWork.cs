namespace Application.Shared.Abstractions;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken ct);
}
