using NotificationLog.RentalService.Domain.Common.ValueObjects;

namespace NotificationLog.RentalService.UnitTests.Domain.Common.ValueObjects;

[TestClass]
public class TimeSlotTests : DomainScenario
{
    private const string OutOfHours =
        "La franja dura una hora y debe empezar entre las 7:00 y las 18:00 hora de Colombia.";

    private TimeSlot _slot = null!;
    private bool _slotsOverlap;

    private string Time { get; set; } = string.Empty;
    private string First { get; set; } = string.Empty;
    private string Second { get; set; } = string.Empty;
    private bool Overlap { get; set; }
    private string Result { get; set; } = string.Empty;

    [TestMethod]
    public void ASlotStartsBetween7And18ColombiaTime() =>
        this.When(_ => ASlotIsCreatedAt(Time), "Cuando se crea una franja que empieza a las <time> hora de Colombia")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("time", "result")
            {
                { "06:59", OutOfHours },
                { "07:00", Accepted },
                { "12:30", Accepted },
                { "18:00", Accepted },
                { "18:01", OutOfHours },
                { "23:30", OutOfHours },
            })
            .BDDfy("Una franja empieza entre las 7:00 y las 18:00 hora de Colombia");

    [TestMethod]
    public void HoursAreEvaluatedInColombiaTimeEvenWhenGivenInUtc() =>
        this.When(_ => ASlotIsCreatedAtUtc(Time), "Cuando se crea una franja que empieza a las <time> UTC")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("time", "result")
            {
                { "11:59", OutOfHours },
                { "12:00", Accepted },
                { "23:00", Accepted },
                { "23:01", OutOfHours },
            })
            .BDDfy("El horario se evalúa en hora de Colombia aunque la hora llegue en UTC");

    [TestMethod]
    public void ASlotLastsOneHour() =>
        this.When(_ => ASlotIsCreatedAt("10:00"), "Cuando se crea una franja que empieza a las 10:00")
            .Then(_ => EndsAt("11:00"), "Entonces termina a las 11:00")
            .BDDfy("Una franja dura una hora");

    [TestMethod]
    public void TwoSlotsOverlapWhenTheyShareAnyMinute() =>
        this.When(_ => SlotsAreCompared(First, Second), "Cuando se comparan las franjas de las <first> y de las <second>")
            .Then(_ => OverlapIs(Overlap), "Entonces se solapan: <overlap>")
            .WithExamples(new ExampleTable("first", "second", "overlap")
            {
                { "10:00", "10:00", true },
                { "10:00", "10:30", true },
                { "10:00", "09:01", true },
                { "10:00", "11:00", false },
                { "10:00", "09:00", false },
                { "10:00", "15:00", false },
            })
            .BDDfy("Dos franjas se solapan si comparten algún minuto");

    private void ASlotIsCreatedAt(string time) => Try(() => _slot = Clock.Slot($"2026-10-07 {time}"));

    private void ASlotIsCreatedAtUtc(string time)
        => Try(() => _slot = TimeSlot.Create(new DateTimeOffset(Clock.At($"2026-10-07 {time}").DateTime, TimeSpan.Zero)));

    private void EndsAt(string time) => Assert.AreEqual(Clock.At($"2026-10-07 {time}"), _slot.EndsAt);

    private void SlotsAreCompared(string first, string second)
        => _slotsOverlap = Clock.Slot($"2026-10-07 {first}").Overlaps(Clock.Slot($"2026-10-07 {second}"));

    private void OverlapIs(bool expected) => Assert.AreEqual(expected, _slotsOverlap);
}
