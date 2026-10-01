using System.Security.Claims;
using Application.Shared.Abstractions;
using Application.Shared.Common;
using Domain.Shared.Authorization;
using FluentValidation;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Application.Common.Producers;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Visits;

namespace NotificationLog.RentalService.Application.Visits.Commands.CancelVisit;

public sealed class CancelVisitHandler(
    IVisitRepository visits,
    IListingRepository listings,
    IUnitOfWork uow,
    INotificationProducer notifications,
    TimeProvider time,
    IValidator<CancelVisitCommand> validator)
{
    private readonly IVisitRepository _visits = visits;
    private readonly IListingRepository _listings = listings;
    private readonly IUnitOfWork _uow = uow;
    private readonly INotificationProducer _notifications = notifications;
    private readonly TimeProvider _time = time;
    private readonly IValidator<CancelVisitCommand> _validator = validator;

    public async Task HandleAsync(CancelVisitCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var visit = await _visits.GetAsync(cmd.VisitId, ct);
        var party = visit.PartyOf(user.GetUserId());

        visit.Cancel(party, cmd.Reason, _time.GetUtcNow());

        if (visit.DomainEvents.Count == 0)
            return;

        await _visits.AppendAsync(visit, ct);
        await _uow.SaveChangesAsync(ct);

        await _notifications.NotifyAsync(
            VisitNotificationKeys.Cancelled,
            visit.CounterpartOf(party),
            new Dictionary<string, string>(VisitNotificationData.For(visit, await _listings.GetAsync(visit.ListingId, ct)))
            {
                ["reason"] = cmd.Reason.Trim()
            },
            ct);
    }
}
