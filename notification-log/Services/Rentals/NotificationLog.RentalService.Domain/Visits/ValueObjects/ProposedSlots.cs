using Domain.Shared.Exceptions;
using NotificationLog.RentalService.Domain.Shared;

namespace NotificationLog.RentalService.Domain.Visits.ValueObjects;

public sealed record ProposedSlots
{
    public IReadOnlyList<TimeSlot> Slots { get; }

    private ProposedSlots(IReadOnlyList<TimeSlot> slots) => Slots = slots;

    public static ProposedSlots Create(IReadOnlyList<TimeSlot> slots, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(slots);

        if (slots.Count is 0 or > VisitPolicy.MaxSlots)
            throw new DomainException($"Hay que proponer entre 1 y {VisitPolicy.MaxSlots} franjas.");

        foreach (var slot in slots)
        {
            ArgumentNullException.ThrowIfNull(slot);

            if (slot.Duration != VisitPolicy.SlotDuration)
                throw new DomainException($"Cada franja debe durar {VisitPolicy.SlotDuration.TotalMinutes} minutos.");

            if (slot.Start < now + VisitPolicy.MinLeadTime)
                throw new DomainException(
                    $"Las franjas deben empezar con al menos {VisitPolicy.MinLeadTime.TotalHours} horas de antelación.");

            if (slot.Start > now + VisitPolicy.MaxLeadTime)
                throw new DomainException(
                    $"Las franjas no pueden estar a más de {VisitPolicy.MaxLeadTime.TotalDays} días.");

            var localStart = slot.Start.ToOffset(VisitPolicy.ColombiaOffset);
            var localEnd = slot.End.ToOffset(VisitPolicy.ColombiaOffset);

            if (localStart.TimeOfDay < VisitPolicy.EarliestStart
                || localEnd.TimeOfDay > VisitPolicy.LatestEnd
                || localEnd.Date != localStart.Date)
                throw new DomainException(
                    $"Las visitas son entre las {VisitPolicy.EarliestStart.Hours}:00 y las {VisitPolicy.LatestEnd.Hours}:00, hora de Colombia.");
        }

        var ordered = slots.OrderBy(slot => slot.Start).ToList();

        for (var i = 1; i < ordered.Count; i++)
            if (ordered[i].Overlaps(ordered[i - 1]))
                throw new DomainException("Las franjas propuestas no pueden solaparse.");

        return new ProposedSlots(ordered);
    }

    public bool Equals(ProposedSlots? other) => other is not null && Slots.SequenceEqual(other.Slots);

    public override int GetHashCode() => Slots.Aggregate(0, (hash, slot) => HashCode.Combine(hash, slot));
}
