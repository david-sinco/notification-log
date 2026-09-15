using Domain.Shared.Common;
using Domain.Shared.EventSourcing;
using Domain.Shared.Exceptions;
using NotificationLog.RentalService.Domain.Shared;
using NotificationLog.RentalService.Domain.Visits.Events;
using NotificationLog.RentalService.Domain.Visits.ValueObjects;
using NotificationLog.RentalService.Domain.Visits.Enums;

namespace NotificationLog.RentalService.Domain.Visits;

public sealed class Visit : AggregateRoot
{
    private readonly List<TimeSlot> _slots = [];
    private TimeSpan _slotDuration;

    private Visit() { }

    public Guid ListingId { get; private set; }
    public Guid VisitorId { get; private set; }
    public Guid HostId { get; private set; }
    public VisitStatus Status { get; private set; }
    public TimeSlot? ConfirmedSlot { get; private set; }
    public DateTimeOffset RespondBy { get; private set; }
    public VisitParty? CancelledBy { get; private set; }
    public bool IsLateCancellation { get; private set; }
    public IReadOnlyList<TimeSlot> Slots => _slots.AsReadOnly();

    public static Visit Request(
        Guid id,
        Guid listingId,
        Guid visitorId,
        Guid hostId,
        ProposedSlots proposedSlots,
        DateTimeOffset now)
    {
        Guard.RequireId(id, "El identificador de la visita es obligatorio.");
        Guard.RequireId(listingId, "La publicación de la visita es obligatoria.");
        Guard.RequireId(visitorId, "El interesado es obligatorio.");
        Guard.RequireId(hostId, "El anfitrión es obligatorio.");
        ArgumentNullException.ThrowIfNull(proposedSlots);

        if (visitorId == hostId)
            throw new DomainException("No se puede solicitar una visita a una publicación propia.");

        var firstStart = proposedSlots.Slots.Min(slot => slot.Start);
        var deadline = now + VisitPolicy.ResponseWindow;

        var visit = new Visit();

        visit.Raise(new VisitRequested(
            id,
            listingId,
            visitorId,
            hostId,
            proposedSlots.Slots.Select(slot => slot.Start).ToList(),
            VisitPolicy.SlotDuration,
            deadline < firstStart ? deadline : firstStart));

        return visit;
    }

    public void Confirm(DateTimeOffset slotStart, DateTimeOffset now)
    {
        if (Status == VisitStatus.Confirmed && ConfirmedSlot?.Start == slotStart)
            return;

        if (Status != VisitStatus.Requested)
            throw new DomainException("Solo se puede confirmar una visita solicitada.");

        if (now >= RespondBy)
            throw new DomainException("El plazo para confirmar la visita ya venció.");

        var slot = _slots.FirstOrDefault(candidate => candidate.Start == slotStart)
            ?? throw new DomainException("La franja elegida no es una de las propuestas.");

        if (slot.Start <= now)
            throw new DomainException("La franja elegida ya pasó.");

        Raise(new VisitConfirmed(slot.Start));
    }

    public void Decline(string reason)
    {
        if (Status == VisitStatus.Declined)
            return;

        if (Status != VisitStatus.Requested)
            throw new DomainException("Solo se puede rechazar una visita solicitada.");

        Raise(new VisitDeclined(Guard.RequireReason(reason, VisitPolicy.MaxReasonLength)));
    }

    public void ExpireRequest(DateTimeOffset now)
    {
        if (Status != VisitStatus.Requested || now < RespondBy)
            return;

        Raise(new VisitRequestExpired());
    }

    public void Cancel(VisitParty by, string reason, DateTimeOffset now)
    {
        if (!Enum.IsDefined(by))
            throw new DomainException("La parte que cancela no es válida.");

        if (Status == VisitStatus.Cancelled)
            return;

        if (Status is not (VisitStatus.Requested or VisitStatus.Confirmed))
            throw new DomainException("Solo se puede cancelar una visita solicitada o confirmada.");

        if (ConfirmedSlot is { } startedSlot && startedSlot.Start <= now)
            throw new DomainException("La visita ya empezó; hay que registrar su resultado.");

        var isLate = ConfirmedSlot is { } confirmedSlot
            && confirmedSlot.Start - now < VisitPolicy.LateCancellationWindow;

        Raise(new VisitCancelled(by, Guard.RequireReason(reason, VisitPolicy.MaxReasonLength), isLate));
    }

    public void MarkCompleted(DateTimeOffset now) => RecordOutcome(attended: true, now);

    public void MarkNoShow(DateTimeOffset now) => RecordOutcome(attended: false, now);

    public void AutoComplete(DateTimeOffset now)
    {
        if (Status != VisitStatus.Confirmed
            || ConfirmedSlot is not { } slot
            || now < slot.End + VisitPolicy.AutoCompleteAfter)
            return;

        Raise(new VisitCompleted());
    }

    public override void Apply(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case VisitRequested e: When(e); break;
            case VisitConfirmed e: When(e); break;
            case VisitDeclined: Status = VisitStatus.Declined; break;
            case VisitRequestExpired: Status = VisitStatus.Expired; break;
            case VisitCancelled e: When(e); break;
            case VisitCompleted: Status = VisitStatus.Completed; break;
            case VisitNoShow: Status = VisitStatus.NoShow; break;

            default:
                throw new DomainException(
                    $"El evento '{domainEvent.GetType().Name}' no pertenece al agregado Visit.");
        }
    }

    private void When(VisitRequested e)
    {
        Id = e.VisitId;
        ListingId = e.ListingId;
        VisitorId = e.VisitorId;
        HostId = e.HostId;
        _slotDuration = e.SlotDuration;
        _slots.Clear();
        _slots.AddRange(e.SlotStarts.Select(start => TimeSlot.FromStorage(start, start + e.SlotDuration)));
        RespondBy = e.RespondBy;
        Status = VisitStatus.Requested;
    }

    private void When(VisitConfirmed e)
    {
        ConfirmedSlot = TimeSlot.FromStorage(e.SlotStart, e.SlotStart + _slotDuration);
        Status = VisitStatus.Confirmed;
    }

    private void When(VisitCancelled e)
    {
        CancelledBy = e.By;
        IsLateCancellation = e.IsLate;
        Status = VisitStatus.Cancelled;
    }

    private void RecordOutcome(bool attended, DateTimeOffset now)
    {
        if (Status == (attended ? VisitStatus.Completed : VisitStatus.NoShow))
            return;

        if (Status != VisitStatus.Confirmed || ConfirmedSlot is not { } slot)
            throw new DomainException("Solo se registra el resultado de una visita confirmada.");

        if (now < slot.End)
            throw new DomainException("La visita todavía no ha terminado.");

        Raise(attended ? new VisitCompleted() : new VisitNoShow());
    }
}
