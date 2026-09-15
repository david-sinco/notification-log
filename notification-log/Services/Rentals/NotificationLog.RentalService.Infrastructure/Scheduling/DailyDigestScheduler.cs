using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NotificationLog.RentalService.Application.Processes;
using NotificationLog.RentalService.Domain.Visits;

namespace NotificationLog.RentalService.Infrastructure.Scheduling;

internal sealed class DailyDigestScheduler : BackgroundService
{
    private static readonly TimeSpan SendAt = TimeSpan.FromHours(7);

    private readonly IServiceScopeFactory _scopes;
    private readonly TimeProvider _time;
    private readonly ILogger<DailyDigestScheduler> _logger;

    public DailyDigestScheduler(IServiceScopeFactory scopes, TimeProvider time, ILogger<DailyDigestScheduler> logger)
        => (_scopes, _time, _logger) = (scopes, time, logger);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = _time.GetUtcNow();
            var next = NextRun(now);

            await Task.Delay(next - now, _time, stoppingToken);

            try
            {
                await using var scope = _scopes.CreateAsyncScope();
                var matching = scope.ServiceProvider.GetRequiredService<SavedSearchMatchingProcess>();

                await matching.SendDailyDigestAsync(next.AddDays(-1), stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Falló el envío del resumen diario de búsquedas guardadas.");
            }
        }
    }

    private static DateTimeOffset NextRun(DateTimeOffset now)
    {
        var local = now.ToOffset(VisitPolicy.ColombiaOffset);
        var today = new DateTimeOffset(local.Date + SendAt, VisitPolicy.ColombiaOffset);

        return today > now ? today : today.AddDays(1);
    }
}
