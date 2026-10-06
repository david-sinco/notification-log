using NotificationLog.RentalService.Domain.Visits;
using NotificationLog.RentalService.Domain.Visits.Enums;

namespace NotificationLog.RentalService.UnitTests.Support;

public static class VisitFactory
{
    public const string Slot = "2026-10-07 10:00";

    public static readonly Guid Host = Guid.NewGuid();
    public static readonly Guid Visitor = Guid.NewGuid();

    public static Visit Requested(string slots = Slot)
        => Clean(Visit.Request(Guid.NewGuid(), Guid.NewGuid(), Host, Visitor, Clock.Slots(slots), null, Clock.Now));

    public static Visit InStatus(VisitStatus status)
    {
        var visit = Requested();
        var slot = Clock.Slot(Slot);

        switch (status)
        {
            case VisitStatus.AwaitingVisitor:
                visit.CounterPropose(VisitParty.Host, [slot], Clock.Now);
                break;
            case VisitStatus.Scheduled:
                visit.Schedule(VisitParty.Host, slot, Clock.Now);
                break;
            case VisitStatus.Completed:
                visit.Schedule(VisitParty.Host, slot, Clock.Now);
                visit.MarkCompleted(slot.EndsAt);
                break;
            case VisitStatus.NoShow:
                visit.Schedule(VisitParty.Host, slot, Clock.Now);
                visit.MarkNoShow(slot.EndsAt);
                break;
            case VisitStatus.Cancelled:
                visit.Cancel(VisitParty.Host, "El inmueble ya no está disponible", Clock.Now);
                break;
            case VisitStatus.Expired:
                visit.Expire(slot.StartsAt);
                break;
        }

        return Clean(visit);
    }

    private static Visit Clean(Visit visit)
    {
        visit.ClearDomainEvents();

        return visit;
    }
}
