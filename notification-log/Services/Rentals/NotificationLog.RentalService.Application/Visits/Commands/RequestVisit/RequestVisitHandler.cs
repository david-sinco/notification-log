using Application.Shared.Abstractions;
using Application.Shared.Common;
using FluentValidation;
using NotificationLog.RentalService.Application.Common;
using NotificationLog.RentalService.Domain.Visits;
using NotificationLog.RentalService.Application.Abstractions;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;
using NotificationLog.RentalService.Domain.Shared;
using NotificationLog.RentalService.Domain.Visits.ValueObjects;

namespace NotificationLog.RentalService.Application.Visits.Commands.RequestVisit;

public sealed class RequestVisitHandler
{
    private readonly IListingRepository _listings;
    private readonly IVisitRepository _visits;
    private readonly ListingAccess _access;
    private readonly ISoftRuleChecks _rules;
    private readonly IUnitOfWork _uow;
    private readonly IValidator<RequestVisitCommand> _validator;
    private readonly TimeProvider _time;

    public RequestVisitHandler(
        IListingRepository listings,
        IVisitRepository visits,
        ListingAccess access,
        ISoftRuleChecks rules,
        IUnitOfWork uow,
        IValidator<RequestVisitCommand> validator,
        TimeProvider time)
        => (_listings, _visits, _access, _rules, _uow, _validator, _time) = (listings, visits, access, rules, uow, validator, time);

    public async Task<Guid> HandleAsync(RequestVisitCommand cmd, CancellationToken ct)
    {
        await _validator.ValidateAndThrowAppAsync(cmd, ct);

        var listing = await _listings.GetAsync(cmd.ListingId, ct);

        if (listing.Status != ListingStatus.Published)
            throw new AppValidationException("La publicación no está disponible para visitas.");

        if (await _access.CanManageAsync(listing, cmd.VisitorId, ct))
            throw new AppValidationException("No puedes pedir una visita a tu propia publicación.");

        await _access.RequireVerifiedUserAsync(cmd.VisitorId, requireDocument: false, ct);

        if (await _rules.CountPendingVisitsAsync(cmd.VisitorId, ct) >= VisitPolicy.MaxPendingVisits)
            throw new AppValidationException($"Ya tienes {VisitPolicy.MaxPendingVisits} visitas pendientes.");

        var now = _time.GetUtcNow();

        if (await IsBannedAsync(cmd.VisitorId, now, ct))
            throw new AppValidationException(
                $"Por cancelaciones tardías o inasistencias no puedes pedir visitas durante {VisitPolicy.VisitBan.TotalDays} días.");

        var slots = ProposedSlots.Create(
            cmd.SlotStarts.Select(start => TimeSlot.Create(start, start + VisitPolicy.SlotDuration)).ToList(),
            now);

        var visit = Visit.Request(Guid.NewGuid(), listing.Id, cmd.VisitorId, ListingAccess.HostOf(listing), slots, now);

        await _visits.AppendAsync(visit, ct);
        await _uow.SaveChangesAsync(ct);

        return visit.Id;
    }

    private async Task<bool> IsBannedAsync(Guid visitorId, DateTimeOffset now, CancellationToken ct)
    {
        var strikes = (await _rules.GetVisitStrikesAsync(visitorId, now - VisitPolicy.StrikeWindow - VisitPolicy.VisitBan, ct))
            .Order()
            .ToList();

        for (var i = VisitPolicy.MaxStrikes - 1; i < strikes.Count; i++)
        {
            var withinWindow = strikes[i] - strikes[i - (VisitPolicy.MaxStrikes - 1)] <= VisitPolicy.StrikeWindow;

            if (withinWindow && strikes[i] + VisitPolicy.VisitBan > now)
                return true;
        }

        return false;
    }
}
