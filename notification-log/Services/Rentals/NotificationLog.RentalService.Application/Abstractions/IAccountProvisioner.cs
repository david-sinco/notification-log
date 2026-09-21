namespace NotificationLog.RentalService.Application.Abstractions;

public interface IAccountProvisioner
{
    Task RequestAccountAsync(AccountRequest request, CancellationToken ct);
}
