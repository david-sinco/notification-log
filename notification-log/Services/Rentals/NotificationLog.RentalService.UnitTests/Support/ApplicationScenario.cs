using System.Security.Claims;
using Application.Shared.Abstractions;
using Application.Shared.Common;
using Domain.Shared.Authorization;
using Domain.Shared.Common;
using Microsoft.Extensions.DependencyInjection;
using NotificationLog.RentalService.Application;
using NotificationLog.RentalService.Application.Common.Producers;
using NotificationLog.RentalService.Application.Common.Storage;
using NotificationLog.RentalService.Application.Dashboard.Queries;
using NotificationLog.RentalService.Application.Listings.Queries;
using NotificationLog.RentalService.Application.Owners.Queries;
using NotificationLog.RentalService.Application.Visitors.Queries;
using NotificationLog.RentalService.Application.Visits.Queries;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Owners;
using NotificationLog.RentalService.Domain.Visitors;
using NotificationLog.RentalService.Domain.Visits;
using NSubstitute;

namespace NotificationLog.RentalService.UnitTests.Support;

public abstract class ApplicationScenario : DomainScenario
{
    private readonly ServiceProvider _services;

    protected ApplicationScenario()
    {
        var time = Substitute.For<TimeProvider>();
        time.GetUtcNow().Returns(_ => Now);

        _services = new ServiceCollection()
            .AddSingleton(time)
            .AddSingleton(Listings)
            .AddSingleton(Owners)
            .AddSingleton(Visitors)
            .AddSingleton(Visits)
            .AddSingleton(UnitOfWork)
            .AddSingleton(Notifications)
            .AddSingleton(PhotoStorage)
            .AddSingleton(ListingViews)
            .AddSingleton(OwnerViews)
            .AddSingleton(VisitorViews)
            .AddSingleton(VisitViews)
            .AddSingleton(Dashboard)
            .AddApplication()
            .BuildServiceProvider();
    }

    protected IListingRepository Listings { get; } = Substitute.For<IListingRepository>();
    protected IOwnerRepository Owners { get; } = Substitute.For<IOwnerRepository>();
    protected IVisitorRepository Visitors { get; } = Substitute.For<IVisitorRepository>();
    protected IVisitRepository Visits { get; } = Substitute.For<IVisitRepository>();
    protected IUnitOfWork UnitOfWork { get; } = Substitute.For<IUnitOfWork>();
    protected INotificationProducer Notifications { get; } = Substitute.For<INotificationProducer>();
    protected IPhotoStorage PhotoStorage { get; } = Substitute.For<IPhotoStorage>();
    protected IListingReadModel ListingViews { get; } = Substitute.For<IListingReadModel>();
    protected IOwnerReadModel OwnerViews { get; } = Substitute.For<IOwnerReadModel>();
    protected IVisitorReadModel VisitorViews { get; } = Substitute.For<IVisitorReadModel>();
    protected IVisitReadModel VisitViews { get; } = Substitute.For<IVisitReadModel>();
    protected IDashboardReadModel Dashboard { get; } = Substitute.For<IDashboardReadModel>();

    protected ClaimsPrincipal User { get; private set; } = TestUser.With(UserRole.Visitor, Guid.NewGuid());

    protected THandler Handler<THandler>() where THandler : notnull => _services.GetRequiredService<THandler>();

    protected void UserIs(UserRole role, Guid id, string? email = null) => User = TestUser.With(role, id, email);

    protected void OwnerSignsIn() => UserIs(UserRole.Propietario, ListingFactory.Owner);

    protected void AnotherOwnerSignsIn() => UserIs(UserRole.Propietario, Guid.NewGuid());

    protected void ModeratorSignsIn() => UserIs(UserRole.Moderador, ListingFactory.Moderator);

    protected void HostSignsIn() => UserIs(UserRole.Propietario, VisitFactory.Host);

    protected void VisitorSignsIn() => UserIs(UserRole.Visitor, VisitFactory.Visitor);

    protected void AThirdPartySignsIn() => UserIs(UserRole.Visitor, Guid.NewGuid());

    protected void AnyListingIs(Listing listing)
    {
        Listings.LoadAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>()).Returns(listing);
        Exists(OwnerFactory.Natural(listing.OwnerId));
    }

    protected void Exists(Listing listing)
    {
        Listings.LoadAsync(listing.Id, Arg.Any<CancellationToken>()).Returns(listing);
        Exists(OwnerFactory.Natural(listing.OwnerId));
    }

    protected void Exists(Visit visit) => Visits.LoadAsync(visit.Id, Arg.Any<CancellationToken>()).Returns(visit);

    protected void Exists(Owner owner) => Owners.LoadAsync(owner.Id, Arg.Any<CancellationToken>()).Returns(owner);

    protected void Exists(Visitor visitor) => Visitors.LoadAsync(visitor.Id, Arg.Any<CancellationToken>()).Returns(visitor);

    protected override bool IsRejection(Exception exception)
        => base.IsRejection(exception) || exception is AppValidationException or NotFoundException;

    protected void IsForbidden() => Assert.IsInstanceOfType<ForbiddenException>(Rejection);

    protected void IsNotFound() => Assert.IsInstanceOfType<NotFoundException>(Rejection);

    protected void FailsValidationOn(string property)
    {
        Assert.IsInstanceOfType<AppValidationException>(Rejection);
        Assert.IsTrue(((AppValidationException)Rejection).Errors.ContainsKey(property), $"Sin error de validación en '{property}'.");
    }

    protected void IsSaved() => UnitOfWork.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());

    protected void IsSaved(Listing listing)
    {
        Listings.Received(1).AppendAsync(listing, Arg.Any<CancellationToken>());
        IsSaved();
    }

    protected void IsSaved(Visit visit)
    {
        Visits.Received(1).AppendAsync(visit, Arg.Any<CancellationToken>());
        IsSaved();
    }

    protected void NothingIsSaved() => UnitOfWork.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());

    protected void NobodyIsNotified()
        => Notifications.DidNotReceiveWithAnyArgs().NotifyAsync(default!, default, default!, default);

    protected void IsNotified(string eventKey, Guid recipient)
        => Notifications.Received(1).NotifyAsync(
            eventKey, recipient, Arg.Any<IReadOnlyDictionary<string, string>>(), Arg.Any<CancellationToken>());
}
