using NotificationLog.Contracts.Identity;

namespace NotificationLog.RentalService.Application.Owners.Producers;

public interface IAccountProvisioner
{
    Task RequestAccountAsync(AccountCreationRequested request, CancellationToken ct);
}
