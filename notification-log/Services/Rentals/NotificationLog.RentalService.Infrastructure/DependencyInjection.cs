using Application.Shared.Abstractions;
using JasperFx.Events.Daemon;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Owners;
using NotificationLog.RentalService.Domain.Visits;
using NotificationLog.RentalService.Infrastructure.DecisionProjections;
using NotificationLog.RentalService.Infrastructure.IdentityReplica;
using NotificationLog.RentalService.Infrastructure.Messaging;
using NotificationLog.RentalService.Infrastructure.Messaging.Publishers;
using NotificationLog.RentalService.Infrastructure.Messaging.Subscriptions;
using NotificationLog.RentalService.Infrastructure.Persistence;
using NotificationLog.RentalService.Infrastructure.ReadModels;
using NotificationLog.RentalService.Infrastructure.Scheduling;
using Wolverine.Marten;

namespace NotificationLog.RentalService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("rentals")
            ?? throw new InvalidOperationException("Falta la cadena de conexión 'rentals'.");

        services.AddMarten(options => MartenStoreConfiguration.Configure(options, connectionString))
            .UseLightweightSessions()
            .ApplyAllDatabaseChangesOnStartup()
            .IntegrateWithWolverine()
            .AddAsyncDaemon(DaemonMode.Solo)
            .AddSubscriptionWithServices<DomainEventsSubscription>(ServiceLifetime.Singleton);

        services.AddScoped<AggregateStreams>();
        services.AddScoped<IUnitOfWork, MartenUnitOfWork>();
        services.AddScoped<IListingRepository, MartenListingRepository>();
        services.AddScoped<IVisitRepository, MartenVisitRepository>();
        services.AddScoped<IOwnerRepository, MartenOwnerRepository>();

        services.AddScoped<ISoftRuleChecks, MartenSoftRuleChecks>();
        services.AddScoped<IProcessLookups, MartenProcessLookups>();
        services.AddScoped<IIdentityReplica, MartenIdentityReplica>();

        services.AddScoped<IListingReadModel, MartenListingReadModel>();
        services.AddScoped<IVisitReadModel, MartenVisitReadModel>();
        services.AddScoped<IOwnerReadModel, MartenOwnerReadModel>();

        services.AddScoped<ICommandScheduler, WolverineCommandScheduler>();
        services.AddScoped<INotificationDispatcher, WolverineNotificationDispatcher>();
        services.AddScoped<IAccountProvisioner, WolverineAccountProvisioner>();

        services.AddRabbitMqMessaging(configuration);

        return services;
    }
}
