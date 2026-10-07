using System.Security.Claims;
using Domain.Shared.Authorization;
using NotificationLog.RentalService.Application.Owners.Queries.Dtos;

namespace NotificationLog.RentalService.Application.Owners.Queries;

public sealed class ListMyOwnersHandler(IOwnerReadModel owners)
{
    private readonly IOwnerReadModel _owners = owners;

    public Task<IReadOnlyList<OwnerDto>> HandleAsync(ClaimsPrincipal user, CancellationToken ct)
        => _owners.ListRelatedToAsync(user.GetUserId(), ct);
}
