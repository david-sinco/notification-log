using System.Security.Claims;
using Application.Shared.Abstractions;
using Application.Shared.Common;
using Domain.Shared.Authorization;
using FluentValidation;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Application.Common.Producers;
using NotificationLog.RentalService.Domain.Common.ValueObjects;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Visits;

namespace NotificationLog.RentalService.Application.Visits.Commands.CounterProposeVisit;

public sealed class CounterProposeVisitHandler(
    IVisitRepository visits,
    IListingRepository listings,
    IUnitOfWork uow,
    INotificationProducer notifications,
    TimeProvider time,
    IValidator<CounterProposeVisitCommand> validator)
{
    private readonly IVisitRepository _visits = visits;
    private readonly IListingRepository _listings = listings;
    private readonly IUnitOfWork _uow = uow;
    private readonly INotificationProducer _notifications = notifications;
    private readonly TimeProvider _time = time;
    private readonly IValidator<CounterProposeVisitCommand> _validator = validator;

    public async Task HandleAsync(CounterProposeVisitCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var visit = await _visits.GetAsync(cmd.VisitId, ct);
        var party = visit.PartyOf(user.GetUserId());

        visit.CounterPropose(party, cmd.Slots.Select(TimeSlot.Create).ToList(), _time.GetUtcNow());

        await _visits.AppendAsync(visit, ct);
        await _uow.SaveChangesAsync(ct);

        await _notifications.NotifyAsync(
            VisitNotificationKeys.CounterProposed,
            visit.CounterpartOf(party),
            new Dictionary<string, string>(VisitNotificationData.For(visit, await _listings.GetAsync(visit.ListingId, ct)))
            {
                ["slots"] = string.Join(", ", visit.ProposedSlots)
            },
            ct);
    }
}
