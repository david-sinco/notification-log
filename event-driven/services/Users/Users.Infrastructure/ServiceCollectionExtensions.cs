using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Shared.EventLog;
using Users.Application;
using Users.Application.Ports;
using Users.Infrastructure.Materializer;
using Users.Infrastructure.Persistence;

namespace Users.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUsersInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton(TimeProvider.System);

        services.AddSingleton<IEventLog>(_ => new KafkaEventLog(
            configuration["Messaging:BootstrapServers"]
                ?? throw new InvalidOperationException("Missing configuration 'Messaging:BootstrapServers'.")));

        // Registered as a singleton first, then bridged to IHostedService via a factory that
        // resolves the SAME instance — not services.AddHostedService<UserMaterializer>(), which
        // would construct a second, disconnected instance. InMemoryUserRepository and the
        // readiness health check both need the singleton directly.
        services.AddSingleton<UserMaterializer>();
        services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<UserMaterializer>());

        services.AddSingleton<VerificationFunnelMaterializer>();
        services.AddSingleton<IHostedService>(sp => sp.GetRequiredService<VerificationFunnelMaterializer>());

        services.AddScoped<IUserRepository, InMemoryUserRepository>();
        services.AddScoped<UserCommandService>();

        return services;
    }
}
