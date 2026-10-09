using NotificationLog.RentalService.Domain.Visits;
using NotificationLog.RentalService.Domain.Visits.Enums;
using NotificationLog.RentalService.Domain.Visits.Events;

namespace NotificationLog.RentalService.UnitTests.Domain.Visits;

[TestClass]
[TestCategory("Domain-Visits")]
public class VisitRequestTests : DomainScenario
{
    private Visit _visit = null!;

    private string Slots { get; set; } = string.Empty;
    private string Deadline { get; set; } = string.Empty;
    private string Result { get; set; } = string.Empty;

    [TestMethod]
    public void VisitorRequestsAVisit() =>
        this.When(_ => VisitorRequestsWith("2026-10-07 10:00, 2026-10-08 15:00"),
                "Cuando el visitante pide una visita proponiendo las franjas 2026-10-07 10:00 y 2026-10-08 15:00")
            .Then(_ => StatusIs(VisitStatus.AwaitingHost), "Entonces la visita queda esperando al anfitrión")
            .And(_ => HasProposedSlots(2), "Y tiene 2 franjas propuestas")
            .And(_ => RequestIsRecorded(), "Y se registra la solicitud")
            .BDDfy("Un visitante pide una visita proponiendo franjas");

    [TestMethod]
    public void ResponseDeadlineDependsOnTheEarliestSlot() =>
        this.When(_ => VisitorRequestsWith(Slots), "Cuando el visitante pide una visita proponiendo <slots>")
            .Then(_ => DeadlineIs(Deadline), "Entonces el plazo para responder vence el <deadline>")
            .WithExamples(new ExampleTable("slots", "deadline")
            {
                { "2026-10-07 10:00", "2026-10-06 10:00" },
                { "2026-10-08 09:00", "2026-10-07 09:00" },
                { "2026-10-09 08:00", "2026-10-07 09:00" },
                { "2026-10-09 08:00, 2026-10-07 10:00", "2026-10-06 10:00" },
            })
            .BDDfy("El plazo para responder es de 48 horas, o hasta 24 horas antes de la primera franja");

    [TestMethod]
    public void SlotsMustBeBetween24HoursAnd14DaysAhead() =>
        this.When(_ => VisitorRequestsWith(Slots), "Cuando el visitante pide una visita proponiendo <slots>")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("slots", "result")
            {
                { "2026-10-06 08:59", "Cada franja debe empezar en al menos 24 horas." },
                { "2026-10-06 09:00", Accepted },
                { "2026-10-19 09:00", Accepted },
                { "2026-10-19 09:01", "Las franjas no pueden estar a más de 14 días." },
            })
            .BDDfy("Las franjas se proponen con entre 24 horas y 14 días de antelación");

    [TestMethod]
    public void BetweenOneAndThreeSlotsAreProposed() =>
        this.When(_ => VisitorRequestsWith(Slots), "Cuando el visitante pide una visita proponiendo <slots>")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("slots", "result")
            {
                { "", "Hay que proponer entre 1 y 3 franjas." },
                { "2026-10-07 08:00", Accepted },
                { "2026-10-07 08:00, 2026-10-07 10:00, 2026-10-07 12:00", Accepted },
                { "2026-10-07 08:00, 2026-10-07 10:00, 2026-10-07 12:00, 2026-10-07 14:00", "Hay que proponer entre 1 y 3 franjas." },
            })
            .BDDfy("Se proponen entre 1 y 3 franjas");

    [TestMethod]
    public void ProposedSlotsCannotOverlap() =>
        this.When(_ => VisitorRequestsWith(Slots), "Cuando el visitante pide una visita proponiendo <slots>")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("slots", "result")
            {
                { "2026-10-07 10:00, 2026-10-07 11:00", Accepted },
                { "2026-10-07 10:00, 2026-10-07 10:30", "Las franjas propuestas no pueden solaparse." },
                { "2026-10-07 10:00, 2026-10-07 10:00", "Las franjas propuestas no pueden solaparse." },
            })
            .BDDfy("Las franjas propuestas no se solapan");

    [TestMethod]
    public void HostCannotRequestAVisitToTheirOwnListing() =>
        this.When(_ => HostRequestsOwnListing(), "Cuando el anfitrión pide una visita a su propia publicación")
            .Then(_ => IsRejectedWith("No puedes pedir una visita a tu propia publicación."),
                "Entonces se rechaza: No puedes pedir una visita a tu propia publicación.")
            .BDDfy("El anfitrión no puede pedir visita a su propia publicación");

    private void VisitorRequestsWith(string slots) => Request(VisitFactory.Visitor, slots);

    private void HostRequestsOwnListing() => Request(VisitFactory.Host, VisitFactory.Slot);

    private void Request(Guid visitor, string slots)
        => Try(() => _visit = Visit.Request(
            Guid.NewGuid(), Guid.NewGuid(), VisitFactory.Host, visitor, Clock.Slots(slots), null, Now));

    private void StatusIs(VisitStatus expected) => Assert.AreEqual(expected, _visit.Status);

    private void HasProposedSlots(int count) => Assert.HasCount(count, _visit.ProposedSlots);

    private void RequestIsRecorded() => Assert.IsInstanceOfType<VisitRequested>(_visit.DomainEvents.Single());

    private void DeadlineIs(string moment) => Assert.AreEqual(Clock.At(moment), _visit.RespondBy);
}
