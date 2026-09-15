using Domain.Shared.EventSourcing;
using JasperFx.Events.Daemon;
using JasperFx.Events.Projections;
using Marten;
using Marten.Subscriptions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NotificationLog.RentalService.Application.Processes;

namespace NotificationLog.RentalService.Infrastructure.Messaging.Subscriptions;

public sealed class DomainEventsSubscription : SubscriptionBase
{
    private readonly IServiceScopeFactory _scopes;
    private readonly ILogger<DomainEventsSubscription> _logger;

    public DomainEventsSubscription(IServiceScopeFactory scopes, ILogger<DomainEventsSubscription> logger)
    {
        _scopes = scopes;
        _logger = logger;
        Name = "rentals-processes";
    }

    public override async Task<IChangeListener> ProcessEventsAsync(
        EventRange page,
        ISubscriptionController controller,
        IDocumentOperations operations,
        CancellationToken cancellationToken)
    {
        foreach (var e in page.Events)
        {
            if (e.Data is not IDomainEvent domainEvent)
                continue;

            try
            {
                await using var scope = _scopes.CreateAsyncScope();
                var router = scope.ServiceProvider.GetRequiredService<ProcessRouter>();

                await router.RouteAsync(e.StreamId, domainEvent, cancellationToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Falló el proceso del evento {EventType} del flujo {StreamId}.", e.EventTypeName, e.StreamId);
                await controller.RecordDeadLetterEventAsync(e, ex);
            }
        }

        return NullChangeListener.Instance;
    }
}
