using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NotificationLog.RentalService.Infrastructure.Persistence.Views;

namespace NotificationLog.RentalService.Infrastructure.Persistence.Projections;

internal sealed class ViewRebuilder(IDocumentStore store, IConfiguration configuration) : IHostedService
{
    private readonly IDocumentStore _store = store;
    private readonly IConfiguration _configuration = configuration;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_configuration.GetValue<bool>("Rentals:RebuildViews"))
            return;

        using var daemon = await _store.BuildProjectionDaemonAsync();

        await daemon.RebuildProjectionAsync<OwnerView>(cancellationToken);
        await daemon.RebuildProjectionAsync<VisitorView>(cancellationToken);
        await daemon.RebuildProjectionAsync<ListingView>(cancellationToken);
        await daemon.RebuildProjectionAsync<VisitView>(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
