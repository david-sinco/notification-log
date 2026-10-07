using System.Security.Claims;
using Domain.Shared.Authorization;
using NotificationLog.RentalService.Application.Owners.Queries.Dtos;

namespace NotificationLog.RentalService.Application.Owners.Queries;

public sealed class ListClaimableOwnersHandler(IOwnerReadModel owners)
{
    private readonly IOwnerReadModel _owners = owners;

    public async Task<IReadOnlyList<OwnerDto>> HandleAsync(ClaimsPrincipal user, CancellationToken ct)
    {
        if (!user.IsPropietario())
            return [];

        var email = user.GetEmail();
        var phone = user.GetPhone();

        if (string.IsNullOrEmpty(email) && string.IsNullOrEmpty(phone))
            return [];

        return await _owners.ListClaimableAsync(email, phone, ct);
    }
}
