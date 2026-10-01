using Domain.Shared.Common;
using Domain.Shared.EventSourcing;
using Domain.Shared.Exceptions;
using NotificationLog.RentalService.Domain.Common;
using NotificationLog.RentalService.Domain.Common.ValueObjects;
using NotificationLog.RentalService.Domain.Visits.Enums;
using NotificationLog.RentalService.Domain.Visits.Events;
using NotificationLog.RentalService.Domain.Visits.ValueObjects;

namespace NotificationLog.RentalService.Domain.Visits;

public sealed class Visit : AggregateRoot
{
    private readonly List<TimeSlot> _proposedSlots = [];

    private Visit() { }

    public Guid ListingId { get; private set; }
    public Guid HostId { get; private set; }
    public Guid VisitorId { get; private set; }
    public VisitStatus Status { get; private set; }
    public IReadOnlyList<TimeSlot> ProposedSlots => _proposedSlots.AsReadOnly();
    public DateTimeOffset? RespondBy { get; private set; }
    public TimeSlot? ScheduledSlot { get; private set; }
    public VisitNote? Note { get; private set; }
    public VisitParty? CancelledBy { get; private set; }
    public bool IsLateCancellation { get; private set; }
    public VisitParty? ClosedBy { get; private set; }

    private bool IsNegotiating => Status is VisitStatus.AwaitingHost or VisitStatus.AwaitingVisitor;

    public static Visit Request(
        Guid id,
        Guid listingId,
        Guid hostId,
        Guid visitorId,
        IReadOnlyList<TimeSlot> slots,
        VisitNote? note,
        DateTimeOffset now)
    {
        Guard.RequireId(id, "El identificador de la visita es obligatorio.");
        Guard.RequireId(listingId, "La publicación de la visita es obligatoria.");
        Guard.RequireId(hostId, "El anfitrión de la visita es obligatorio.");
        Guard.RequireId(visitorId, "El visitante es obligatorio.");

        if (hostId == visitorId)
            throw new DomainException("No puedes pedir una visita a tu propia publicación.");

        EnsureValidProposal(slots, now);

        var visit = new Visit();

        visit.Raise(new VisitRequested(
            id,
            listingId,
            hostId,
            visitorId,
            slots.Select(slot => slot.StartsAt).ToList(),
            note?.Value,
            RespondByFor(slots, now)), now);

        return visit;
    }

    public VisitParty PartyOf(Guid userId)
    {
        if (userId == HostId)
            return VisitParty.Host;

        if (userId == VisitorId)
            return VisitParty.Visitor;

        throw new ForbiddenException("No participas en esta visita.");
    }

    public Guid CounterpartOf(VisitParty party) => party == VisitParty.Host ? VisitorId : HostId;

    public void CounterPropose(VisitParty by, IReadOnlyList<TimeSlot> slots, DateTimeOffset now)
    {
        EnsureTurn(by, now);
        EnsureValidProposal(slots, now);

        Raise(new VisitCounterProposed(
            by,
            slots.Select(slot => slot.StartsAt).ToList(),
            RespondByFor(slots, now)), now);
    }

    public void Schedule(VisitParty by, TimeSlot slot, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(slot);
        EnsureTurn(by, now);

        if (!_proposedSlots.Contains(slot))
            throw new DomainException("La franja elegida no está entre las propuestas.");

        if (slot.StartsAt - now < VisitPolicy.MinLeadTime)
            throw new DomainException(
                $"La franja debe empezar en al menos {VisitPolicy.MinLeadTime.TotalHours} horas; propón otra.");

        Raise(new VisitScheduled(by, slot.StartsAt), now);
    }

    public void Cancel(VisitParty by, string reason, DateTimeOffset now)
    {
        EnsureParty(by);

        if (Status == VisitStatus.Cancelled)
            return;

        if (!IsNegotiating && Status != VisitStatus.Scheduled)
            throw new DomainException("Solo se puede cancelar una visita en negociación o agendada.");

        if (ScheduledSlot is { } scheduled && scheduled.StartsAt <= now)
            throw new DomainException("La visita ya empezó; no se puede cancelar.");

        var isLate = by == VisitParty.Visitor
            && ScheduledSlot is { } slot
            && slot.StartsAt - now < VisitPolicy.LateCancellationWindow;

        Raise(new VisitCancelled(by, Guard.RequireReason(reason, VisitPolicy.MaxReasonLength), isLate), now);
    }

    public void Expire(DateTimeOffset now)
    {
        if (!IsNegotiating || RespondBy is not { } respondBy || respondBy > now)
            return;

        Raise(new VisitExpired(), now);
    }

    public void MarkCompleted(DateTimeOffset now)
    {
        EnsureCanClose(now);

        Raise(new VisitCompleted(VisitParty.Host), now);
    }

    public void MarkNoShow(DateTimeOffset now)
    {
        EnsureCanClose(now);

        Raise(new VisitNoShow(), now);
    }

    public void AutoComplete(DateTimeOffset now)
    {
        if (Status != VisitStatus.Scheduled
            || ScheduledSlot is not { } slot
            || now - slot.EndsAt < VisitPolicy.AutoCompleteAfter)
            return;

        Raise(new VisitCompleted(VisitParty.System), now);
    }

    public override void Apply(IDomainEvent domainEvent)
    {
        switch (domainEvent)
        {
            case VisitRequested e: When(e); break;
            case VisitCounterProposed e: When(e); break;
            case VisitScheduled e: When(e); break;
            case VisitCancelled e: When(e); break;
            case VisitExpired: Close(VisitStatus.Expired, null); break;
            case VisitCompleted e: Close(VisitStatus.Completed, e.By); break;
            case VisitNoShow: Close(VisitStatus.NoShow, VisitParty.Host); break;

            default:
                throw new DomainException(
                    $"El evento '{domainEvent.GetType().Name}' no pertenece al agregado Visit.");
        }
    }

    private void When(VisitRequested e)
    {
        Id = e.VisitId;
        ListingId = e.ListingId;
        HostId = e.HostId;
        VisitorId = e.VisitorId;
        Note = e.Note is null ? null : VisitNote.FromStorage(e.Note);
        Propose(VisitStatus.AwaitingHost, e.Slots, e.RespondBy);
    }

    private void When(VisitCounterProposed e)
        => Propose(
            e.By == VisitParty.Host ? VisitStatus.AwaitingVisitor : VisitStatus.AwaitingHost,
            e.Slots,
            e.RespondBy);

    private void When(VisitScheduled e)
    {
        Status = VisitStatus.Scheduled;
        ScheduledSlot = TimeSlot.FromStorage(e.StartsAt);
        _proposedSlots.Clear();
        RespondBy = null;
    }

    private void When(VisitCancelled e)
    {
        Status = VisitStatus.Cancelled;
        CancelledBy = e.By;
        IsLateCancellation = e.IsLate;
        _proposedSlots.Clear();
        RespondBy = null;
    }

    private void Propose(VisitStatus status, IReadOnlyList<DateTimeOffset> slots, DateTimeOffset respondBy)
    {
        Status = status;
        _proposedSlots.Clear();
        _proposedSlots.AddRange(slots.Select(TimeSlot.FromStorage));
        RespondBy = respondBy;
    }

    private void Close(VisitStatus status, VisitParty? closedBy)
    {
        Status = status;
        ClosedBy = closedBy;
        _proposedSlots.Clear();
        RespondBy = null;
    }

    private void Raise(DomainEvent domainEvent, DateTimeOffset now)
        => Raise(domainEvent with { OccurredOn = now.UtcDateTime });

    private void EnsureTurn(VisitParty by, DateTimeOffset now)
    {
        EnsureParty(by);

        var expected = Status switch
        {
            VisitStatus.AwaitingHost => VisitParty.Host,
            VisitStatus.AwaitingVisitor => VisitParty.Visitor,
            _ => throw new DomainException("La visita ya no está en negociación.")
        };

        if (by != expected)
            throw new DomainException("No es tu turno de responder a esta visita.");

        if (RespondBy is { } respondBy && respondBy <= now)
            throw new DomainException("El plazo para responder a esta visita ya venció.");
    }

    private void EnsureCanClose(DateTimeOffset now)
    {
        if (Status != VisitStatus.Scheduled || ScheduledSlot is not { } slot)
            throw new DomainException("Solo se puede cerrar una visita agendada.");

        if (slot.StartsAt > now)
            throw new DomainException("La visita todavía no ha empezado.");
    }

    private static void EnsureParty(VisitParty by)
    {
        if (!Enum.IsDefined(by))
            throw new DomainException("La parte de la visita no es válida.");
    }

    private static void EnsureValidProposal(IReadOnlyList<TimeSlot> slots, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(slots);

        if (slots.Any(slot => slot is null))
            throw new ArgumentException("La lista de franjas contiene elementos nulos.", nameof(slots));

        if (slots.Count is 0 or > VisitPolicy.MaxProposedSlots)
            throw new DomainException($"Hay que proponer entre 1 y {VisitPolicy.MaxProposedSlots} franjas.");

        foreach (var slot in slots)
        {
            if (slot.StartsAt - now < VisitPolicy.MinLeadTime)
                throw new DomainException(
                    $"Cada franja debe empezar en al menos {VisitPolicy.MinLeadTime.TotalHours} horas.");

            if (slot.StartsAt - now > VisitPolicy.MaxLeadTime)
                throw new DomainException(
                    $"Las franjas no pueden estar a más de {VisitPolicy.MaxLeadTime.Days} días.");
        }

        for (var i = 0; i < slots.Count; i++)
            for (var j = i + 1; j < slots.Count; j++)
                if (slots[i].Overlaps(slots[j]))
                    throw new DomainException("Las franjas propuestas no pueden solaparse.");
    }

    private static DateTimeOffset RespondByFor(IReadOnlyList<TimeSlot> slots, DateTimeOffset now)
        => VisitPolicy.RespondByFrom(now, slots.Min(slot => slot.StartsAt));
}
