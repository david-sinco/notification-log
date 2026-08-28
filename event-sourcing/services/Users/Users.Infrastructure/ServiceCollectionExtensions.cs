using JasperFx.Events.Daemon;
using JasperFx.Events.Projections;
using Marten;
using Marten.Events.Projections;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Shared.Messaging;
using Users.Application;
using Users.Application.Ports;
using Users.Application.ReadModels;
using Users.Domain;
using Users.Infrastructure.Outbox;
using Users.Infrastructure.Persistence;
using Users.Infrastructure.Projections;

namespace Users.Infrastructure;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddUsersInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMarten(options =>
        {
            options.Connection(configuration.GetConnectionString("usersdb")
                ?? throw new InvalidOperationException("Missing connection string 'usersdb'."));

            // Two projection profiles, deliberately different lifecycles (SPEC.md §4): the cheap,
            // read-after-write-consistent baseline, and the expensive multi-stream one the async
            // daemon has to walk single-threaded — do not simplify either one away.
            options.Projections.Snapshot<User>(SnapshotLifecycle.Inline);
            options.Projections.Snapshot<UserProfile>(SnapshotLifecycle.Inline);
            options.Projections.Add(new VerificationFunnelProjection(), ProjectionLifecycle.Async);
        }).AddAsyncDaemon(DaemonMode.Solo);

        services.TryAddSingleton(TimeProvider.System);

        services.AddScoped<OutboxBuffer>();
        services.AddScoped<IIntegrationEventOutbox>(sp => sp.GetRequiredService<OutboxBuffer>());
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<UserCommandService>();

        services.AddSingleton<IMessageTransport>(_ => MessageTransportFactory.Create(
            configuration["Messaging:Kind"] ?? "RabbitMq",
            configuration["Messaging:ConnectionString"]
                ?? throw new InvalidOperationException("Missing configuration 'Messaging:ConnectionString'."),
            configuration.GetValue("Messaging:StreamPort", 5552)));

        services.AddHostedService<OutboxDispatcher>();

        return services;
    }
}
