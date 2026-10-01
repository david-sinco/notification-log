using System.Security.Claims;
using Application.Shared.Abstractions;
using Domain.Shared.Authorization;
using Domain.Shared.Common;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Application.Common.Producers;
using NotificationLog.RentalService.Domain.Visits;
using NotificationLog.RentalService.Domain.Visits.Enums;

namespace NotificationLog.RentalService.Application.Visits.Commands.MarkVisitCompleted;

public sealed class MarkVisitCompletedHandler(
    IVisitRepository visits,
    IUnitOfWork uow,
    INotificationProducer notifications,
    TimeProvider time)
{
    private readonly IVisitRepository _visits = visits;
    private readonly IUnitOfWork _uow = uow;
    private readonly INotificationProducer _notifications = notifications;
    private readonly TimeProvider _time = time;

    public async Task HandleAsync(MarkVisitCompletedCommand cmd, ClaimsPrincipal user, CancellationToken ct)
    {
        var visit = await _visits.GetAsync(cmd.VisitId, ct);

        if (visit.PartyOf(user.GetUserId()) != VisitParty.Host)
            throw new ForbiddenException("Solo el anfitrión puede cerrar la visita.");

        visit.MarkCompleted(_time.GetUtcNow());

        await _visits.AppendAsync(visit, ct);
        await _uow.SaveChangesAsync(ct);

        await _notifications.NotifyAsync(
            VisitNotificationKeys.Completed,
            visit.VisitorId,
            new Dictionary<string, string>
            {
                ["visit_id"] = visit.Id.ToString(),
                ["listing_id"] = visit.ListingId.ToString()
            },
            ct);
    }
}
