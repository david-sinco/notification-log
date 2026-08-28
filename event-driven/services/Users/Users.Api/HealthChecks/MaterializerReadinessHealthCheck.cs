using Microsoft.Extensions.Diagnostics.HealthChecks;
using Users.Infrastructure.Materializer;

namespace Users.Api.HealthChecks;

/// <summary>
/// Wired into /health so Aspire's WithHttpHealthCheck — and anyone curling /health by hand — sees
/// this service as unhealthy until both materializers have replayed users.events through to the
/// high-watermark each observed at startup. Individual requests are also gated directly
/// (InMemoryUserRepository awaits WaitUntilReadyAsync), so this mainly matters for AppHost
/// startup ordering and for a human checking "is it done replaying yet."
/// </summary>
public sealed class MaterializerReadinessHealthCheck(
    UserMaterializer users, VerificationFunnelMaterializer funnel) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken ct) =>
        Task.FromResult(users.IsReady && funnel.IsReady
            ? HealthCheckResult.Healthy("Replay caught up to the high-watermark.")
            : HealthCheckResult.Unhealthy("Still replaying users.events from the beginning."));
}
