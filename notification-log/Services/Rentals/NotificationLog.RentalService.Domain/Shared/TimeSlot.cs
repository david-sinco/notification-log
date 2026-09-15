using Domain.Shared.Exceptions;

namespace NotificationLog.RentalService.Domain.Shared;

public sealed record TimeSlot
{
    public DateTimeOffset Start { get; }
    public DateTimeOffset End { get; }
    public TimeSpan Duration => End - Start;

    private TimeSlot(DateTimeOffset start, DateTimeOffset end)
    {
        Start = start;
        End = end;
    }

    public static TimeSlot Create(DateTimeOffset start, DateTimeOffset end)
    {
        if (end <= start)
            throw new DomainException("El final de la franja debe ser posterior a su inicio.");

        return new TimeSlot(start, end);
    }

    internal static TimeSlot FromStorage(DateTimeOffset start, DateTimeOffset end) => new(start, end);

    public bool Overlaps(TimeSlot other) => Start < other.End && other.Start < End;
}
