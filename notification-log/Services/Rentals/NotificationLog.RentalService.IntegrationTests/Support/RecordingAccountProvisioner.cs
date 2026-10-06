using System.Collections.Concurrent;
using NotificationLog.Contracts.Identity;
using NotificationLog.RentalService.Application.Common.Producers;

namespace NotificationLog.RentalService.IntegrationTests.Support;

public sealed class RecordingAccountProvisioner : IAccountProvisioner
{
    private readonly ConcurrentQueue<AccountCreationRequested> _requested = new();

    public IReadOnlyList<AccountCreationRequested> Requested => _requested.ToList();

    public Task RequestAccountAsync(AccountCreationRequested request, CancellationToken ct)
    {
        _requested.Enqueue(request);

        return Task.CompletedTask;
    }

    public void Clear() => _requested.Clear();
}
