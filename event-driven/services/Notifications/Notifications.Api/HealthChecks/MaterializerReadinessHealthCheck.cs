using Microsoft.Extensions.Diagnostics.HealthChecks;
using Notifications.Infrastructure.EventLog;

namespace Notifications.Api.HealthChecks;

/// <summary>Same purpose as Users.Api's equivalent: /health stays unhealthy until the contact
/// materializer has replayed users.events through to the high-watermark it observed at startup.</summary>
public sealed class MaterializerReadinessHealthCheck(ContactMaterializer contacts) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct) =>
        Task.FromResult(contacts.IsReady
            ? HealthCheckResult.Healthy("Replay caught up to the high-watermark.")
            : HealthCheckResult.Unhealthy("Still replaying users.events from the beginning."));
}
