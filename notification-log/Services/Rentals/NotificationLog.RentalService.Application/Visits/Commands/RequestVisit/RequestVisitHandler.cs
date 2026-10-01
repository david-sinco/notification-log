using System.Security.Claims;
using Application.Shared.Abstractions;
using Application.Shared.Common;
using Domain.Shared.Authorization;
using FluentValidation;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Application.Common.Producers;
using NotificationLog.RentalService.Domain.Common.ValueObjects;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;
using NotificationLog.RentalService.Domain.Visitors;
using NotificationLog.RentalService.Domain.Visitors.Enums;
using NotificationLog.RentalService.Domain.Visits;

namespace NotificationLog.RentalService.Application.Visits.Commands.RequestVisit;

public sealed class RequestVisitHandler(
    IVisitRepository visits,
    IListingRepository listings,
    IVisitorRepository visitors,
    IUnitOfWork uow,
    INotificationProducer notifications,
    TimeProvider time,
    IValidator<RequestVisitCommand> validator)
{
    private readonly IVisitRepository _visits = visits;
    private readonly IListingRepository _listings = listings;
    private readonly IVisitorRepository _visitors = visitors;
    private readonly IUnitOfWork _uow = uow;
    private readonly INotificationProducer _notifications = notifications;
    private readonly TimeProvider _time = time;
    private readonly IValidator<RequestVisitCommand> _validator = validator;

    public async Task<Guid> HandleAsync(RequestVisitCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var visitorId = user.GetUserId();

        if (await _visitors.LoadAsync(visitorId, ct) is not { Status: VisitorStatus.Registered })
            throw new AppValidationException("Completa tu perfil de visitante antes de pedir una visita.");

        var listing = await _listings.GetAsync(cmd.ListingId, ct);

        if (listing.Status != ListingStatus.Published)
            throw new AppValidationException("Solo se pueden visitar publicaciones publicadas.");

        var visit = Visit.Request(
            Guid.NewGuid(),
            listing.Id,
            Listing.HostOf(listing),
            visitorId,
            cmd.Slots.Select(TimeSlot.Create).ToList(),
            null,
            _time.GetUtcNow());

        await _visits.AppendAsync(visit, ct);
        await _uow.SaveChangesAsync(ct);

        await _notifications.NotifyAsync(
            VisitNotificationKeys.Requested,
            visit.HostId,
            new Dictionary<string, string>
            {
                ["visit_id"] = visit.Id.ToString(),
                ["listing_id"] = visit.ListingId.ToString(),
                ["slots"] = string.Join(", ", visit.ProposedSlots)
            },
            ct);

        return visit.Id;
    }
}
