using Application.Shared.Abstractions;
using JasperFx.Events.Daemon;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Application.Listings.Queries;
using NotificationLog.RentalService.Application.Owners.Queries;
using NotificationLog.RentalService.Application.Visitors.Queries;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Owners;
using NotificationLog.RentalService.Domain.Visitors;
using NotificationLog.RentalService.Infrastructure.Messaging;
using NotificationLog.RentalService.Infrastructure.Messaging.Publishers;
using NotificationLog.RentalService.Infrastructure.Persistence;
using NotificationLog.RentalService.Infrastructure.ReadModels;
using NotificationLog.RentalService.Infrastructure.Scheduling;
using Wolverine.Marten;
using NotificationLog.RentalService.Application.Common.Producers;

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
            .AddAsyncDaemon(DaemonMode.Solo);

        services.AddScoped<AggregateStreams>();
        services.AddScoped<IUnitOfWork, MartenUnitOfWork>();
        services.AddScoped<IListingRepository, MartenListingRepository>();
        services.AddScoped<IOwnerRepository, MartenOwnerRepository>();
        services.AddScoped<IVisitorRepository, MartenVisitorRepository>();

        services.AddScoped<IListingReadModel, MartenListingReadModel>();
        services.AddScoped<IOwnerReadModel, MartenOwnerReadModel>();
        services.AddScoped<IVisitorReadModel, MartenVisitorReadModel>();

        services.AddScoped<ICommandScheduler, WolverineCommandScheduler>();
        services.AddScoped<INotificationProducer, WolverineNotificationDispatcher>();
        services.AddScoped<IAccountProvisioner, WolverineAccountProvisioner>();

        services.AddRabbitMqMessaging(configuration);

        return services;
    }
}
