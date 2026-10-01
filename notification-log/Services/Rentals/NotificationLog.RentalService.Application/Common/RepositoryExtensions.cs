using Application.Shared.Common;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Visits;

namespace NotificationLog.RentalService.Application.Common;

public static class RepositoryExtensions
{
    public static async Task<Listing> GetAsync(this IListingRepository repository, Guid id, CancellationToken ct)
        => await repository.LoadAsync(id, ct) ?? throw new NotFoundException(nameof(Listing), id);

    public static async Task<Visit> GetAsync(this IVisitRepository repository, Guid id, CancellationToken ct)
        => await repository.LoadAsync(id, ct) ?? throw new NotFoundException(nameof(Visit), id);
}
