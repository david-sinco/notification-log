using NotificationLog.RentalService.Infrastructure.Persistence.Projections;
using NotificationLog.RentalService.Infrastructure.Persistence.ReadRepositories;
using NotificationLog.RentalService.Infrastructure.Persistence.Repositories;
using Application.Shared.Abstractions;
using JasperFx.Events.Daemon;
using Marten;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationLog.RentalService.Application.Dashboard.Queries;
using NotificationLog.RentalService.Application.Listings.Queries;
using NotificationLog.RentalService.Application.Owners.Queries;
using NotificationLog.RentalService.Application.Visitors.Queries;
using NotificationLog.RentalService.Application.Visits.Queries;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Owners;
using NotificationLog.RentalService.Domain.Visitors;
using NotificationLog.RentalService.Domain.Visits;
using NotificationLog.RentalService.Infrastructure.Messaging;
using NotificationLog.RentalService.Infrastructure.Messaging.Publishers;
using NotificationLog.RentalService.Infrastructure.Persistence;
using NotificationLog.RentalService.Infrastructure.Storage;
using NotificationLog.RentalService.Application.Common.Storage;
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

        services.AddHostedService<ViewRebuilder>();

        services.AddScoped<AggregateStreams>();
        services.AddScoped<IUnitOfWork, MartenUnitOfWork>();
        services.AddScoped<IListingRepository, MartenListingRepository>();
        services.AddScoped<IOwnerRepository, MartenOwnerRepository>();
        services.AddScoped<IVisitorRepository, MartenVisitorRepository>();
        services.AddScoped<IVisitRepository, MartenVisitRepository>();

        services.AddScoped<IListingReadModel, MartenListingReadModel>();
        services.AddScoped<IOwnerReadModel, MartenOwnerReadModel>();
        services.AddScoped<IVisitorReadModel, MartenVisitorReadModel>();
        services.AddScoped<IVisitReadModel, MartenVisitReadModel>();
        services.AddScoped<IDashboardReadModel, MartenDashboardReadModel>();

        services.AddScoped<INotificationProducer, WolverineNotificationDispatcher>();

        services.AddSingleton<AzureBlobPhotoStorage>();
        services.AddSingleton<IPhotoStorage>(provider => provider.GetRequiredService<AzureBlobPhotoStorage>());
        services.AddSingleton<IPhotoUrlProvider>(provider => provider.GetRequiredService<AzureBlobPhotoStorage>());

        services.AddRabbitMqMessaging(configuration);

        return services;
    }
}
