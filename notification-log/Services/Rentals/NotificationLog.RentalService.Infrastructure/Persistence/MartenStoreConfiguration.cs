using Domain.Shared.EventSourcing;
using JasperFx.Events.Projections;
using Marten;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Infrastructure.DecisionProjections;
using NotificationLog.RentalService.Infrastructure.IdentityReplica;
using NotificationLog.RentalService.Infrastructure.ReadModels;
using Weasel.Core;

namespace NotificationLog.RentalService.Infrastructure.Persistence;

internal static class MartenStoreConfiguration
{
    public static void Configure(StoreOptions options, string connectionString)
    {
        options.Connection(connectionString);
        options.DatabaseSchemaName = "rentals";
        options.UseSystemTextJsonForSerialization(EnumStorage.AsString);

        options.Events.AddEventTypes(typeof(Listing).Assembly.GetTypes()
            .Where(type => type is { IsAbstract: false, IsClass: true } && typeof(IDomainEvent).IsAssignableFrom(type)));

        options.Projections.Add(new ListingDecisionProjection(), ProjectionLifecycle.Inline);
        options.Projections.Add(new OfferDecisionProjection(), ProjectionLifecycle.Inline);
        options.Projections.Add(new VisitDecisionProjection(), ProjectionLifecycle.Inline);

        options.Projections.Add(new ListingViewProjection(), ProjectionLifecycle.Inline);
        options.Projections.Add(new OfferViewProjection(), ProjectionLifecycle.Inline);
        options.Projections.Add(new VisitViewProjection(), ProjectionLifecycle.Inline);
        options.Projections.Add(new OwnerViewProjection(), ProjectionLifecycle.Inline);

        options.Schema.For<PersonVerificationDocument>().Index(x => x.UserId);
        options.Schema.For<AdvisorDocument>();
    }
}
