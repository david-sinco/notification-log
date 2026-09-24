using NotificationLog.Contracts.Identity;

namespace NotificationLog.RentalService.Application.Common.Producers;

public interface IAccountProvisioner
{
    Task RequestAccountAsync(AccountCreationRequested request, CancellationToken ct);
}
