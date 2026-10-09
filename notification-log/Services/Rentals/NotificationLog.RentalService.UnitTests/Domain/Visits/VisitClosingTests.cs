using NotificationLog.RentalService.Domain.Visits;
using NotificationLog.RentalService.Domain.Visits.Enums;

namespace NotificationLog.RentalService.UnitTests.Domain.Visits;

[TestClass]
[TestCategory("Domain-Visits")]
public class VisitClosingTests : DomainScenario
{
    private Visit _visit = null!;

    private VisitStatus Status { get; set; }
    private VisitStatus Becomes { get; set; }
    private string Moment { get; set; } = string.Empty;
    private string Result { get; set; } = string.Empty;

    [TestMethod]
    public void HostMarksTheVisitAsCompleted() =>
        this.Given(_ => AVisitScheduledForOctober7At10(), "Dada una visita agendada para el 2026-10-07 10:00")
            .And(_ => ItIs("2026-10-07 11:30"), "Y son las 2026-10-07 11:30")
            .When(_ => IsMarkedAsCompleted(), "Cuando el anfitrión la marca como realizada")
            .Then(_ => StatusIs(VisitStatus.Completed), "Entonces la visita queda realizada")
            .And(_ => WasClosedBy(VisitParty.Host), "Y queda registrado que la cerró el anfitrión")
            .BDDfy("El anfitrión marca la visita como realizada");

    [TestMethod]
    public void HostMarksTheVisitorAsNoShow() =>
        this.Given(_ => AVisitScheduledForOctober7At10(), "Dada una visita agendada para el 2026-10-07 10:00")
            .And(_ => ItIs("2026-10-07 11:30"), "Y son las 2026-10-07 11:30")
            .When(_ => IsMarkedAsNoShow(), "Cuando el anfitrión marca que el visitante no asistió")
            .Then(_ => StatusIs(VisitStatus.NoShow), "Entonces la visita queda como inasistencia")
            .And(_ => WasClosedBy(VisitParty.Host), "Y queda registrado que la cerró el anfitrión")
            .BDDfy("El anfitrión marca que el visitante no asistió");

    [TestMethod]
    public void AVisitCanOnlyBeClosedFromItsScheduledTime() =>
        this.Given(_ => AVisitScheduledForOctober7At10(), "Dada una visita agendada para el 2026-10-07 10:00")
            .And(_ => ItIs(Moment), "Y son las <moment>")
            .When(_ => IsMarkedAsCompleted(), "Cuando el anfitrión la marca como realizada")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("moment", "result")
            {
                { "2026-10-07 09:59", "La visita todavía no ha empezado." },
                { "2026-10-07 10:00", Accepted },
            })
            .BDDfy("Solo se cierra una visita desde la hora agendada");

    [TestMethod]
    public void OnlyAScheduledVisitCanBeClosed() =>
        this.Given(_ => AVisitInStatus(Status), "Dada una visita en estado <status>")
            .And(_ => ItIs("2026-10-07 11:30"), "Y son las 2026-10-07 11:30")
            .When(_ => IsMarkedAsNoShow(), "Cuando el anfitrión marca que el visitante no asistió")
            .Then(_ => IsRejectedWith("Solo se puede cerrar una visita agendada."),
                "Entonces se rechaza: Solo se puede cerrar una visita agendada.")
            .WithExamples(new ExampleTable("status")
            {
                { VisitStatus.AwaitingHost },
                { VisitStatus.AwaitingVisitor },
                { VisitStatus.Completed },
                { VisitStatus.NoShow },
                { VisitStatus.Cancelled },
                { VisitStatus.Expired },
            })
            .BDDfy("Solo se cierra una visita agendada");

    [TestMethod]
    public void AVisitIsAutoCompleted72HoursAfterItEnds() =>
        this.Given(_ => AVisitScheduledForOctober7At10(), "Dada una visita agendada para el 2026-10-07 10:00, que termina a las 11:00")
            .And(_ => ItIs(Moment), "Y son las <moment>")
            .When(_ => SystemChecksTheVisit(), "Cuando el sistema revisa la visita")
            .Then(_ => StatusIs(Becomes), "Entonces la visita queda en estado <becomes>")
            .WithExamples(new ExampleTable("moment", "becomes")
            {
                { "2026-10-10 10:59", VisitStatus.Scheduled },
                { "2026-10-10 11:00", VisitStatus.Completed },
            })
            .BDDfy("La visita se da por realizada 72 horas después de terminar");

    [TestMethod]
    public void AnAutoCompletedVisitIsClosedByTheSystem() =>
        this.Given(_ => AVisitScheduledForOctober7At10(), "Dada una visita agendada para el 2026-10-07 10:00")
            .And(_ => ItIs("2026-10-10 11:00"), "Y son las 2026-10-10 11:00")
            .When(_ => SystemChecksTheVisit(), "Cuando el sistema revisa la visita")
            .Then(_ => WasClosedBy(VisitParty.System), "Entonces queda registrado que la cerró el sistema")
            .BDDfy("La visita que se da por realizada la cierra el sistema");

    [TestMethod]
    public void AnUnscheduledVisitIsNotAutoCompleted() =>
        this.Given(_ => AVisitInStatus(VisitStatus.AwaitingHost), "Dada una visita esperando al anfitrión")
            .And(_ => ItIs("2026-10-10 11:00"), "Y son las 2026-10-10 11:00")
            .When(_ => SystemChecksTheVisit(), "Cuando el sistema revisa la visita")
            .Then(_ => StatusIs(VisitStatus.AwaitingHost), "Entonces la visita sigue esperando al anfitrión")
            .BDDfy("El sistema no da por realizada una visita sin agendar");

    private void AVisitScheduledForOctober7At10() => _visit = VisitFactory.InStatus(VisitStatus.Scheduled);

    private void AVisitInStatus(VisitStatus status) => _visit = VisitFactory.InStatus(status);

    private void IsMarkedAsCompleted() => Try(() => _visit.MarkCompleted(Now));

    private void IsMarkedAsNoShow() => Try(() => _visit.MarkNoShow(Now));

    private void SystemChecksTheVisit() => _visit.AutoComplete(Now);

    private void StatusIs(VisitStatus expected) => Assert.AreEqual(expected, _visit.Status);

    private void WasClosedBy(VisitParty who) => Assert.AreEqual(who, _visit.ClosedBy);
}
