using NotificationLog.RentalService.Domain.Visits;
using NotificationLog.RentalService.Domain.Visits.Enums;

namespace NotificationLog.RentalService.UnitTests.Domain.Visits;

[TestClass]
[TestCategory("Domain-Visits")]
public class VisitNegotiationTests : DomainScenario
{
    private Visit _visit = null!;

    private VisitStatus Status { get; set; }
    private VisitParty Who { get; set; }
    private VisitStatus Becomes { get; set; }
    private string Moment { get; set; } = string.Empty;
    private string Result { get; set; } = string.Empty;

    [TestMethod]
    public void AcceptingAProposedSlotSchedulesTheVisit() =>
        this.Given(_ => AVisitRequestedWith("2026-10-07 10:00, 2026-10-08 15:00"),
                "Dada una visita solicitada con las franjas 2026-10-07 10:00 y 2026-10-08 15:00")
            .When(_ => Schedules(VisitParty.Host, "2026-10-07 10:00"), "Cuando el anfitrión acepta la franja 2026-10-07 10:00")
            .Then(_ => StatusIs(VisitStatus.Scheduled), "Entonces la visita queda agendada")
            .And(_ => IsScheduledFor("2026-10-07 10:00"), "Y la franja agendada es 2026-10-07 10:00")
            .And(_ => HasNoProposedSlotsNorDeadline(), "Y ya no tiene franjas propuestas ni plazo para responder")
            .BDDfy("Aceptar una franja propuesta agenda la visita");

    [TestMethod]
    public void CounterProposingPassesTheTurn() =>
        this.Given(_ => AVisitInStatus(Status), "Dada una visita en estado <status>")
            .When(_ => CounterProposes(Who, "2026-10-09 08:00"), "Cuando <who> propone la franja 2026-10-09 08:00")
            .Then(_ => StatusIs(Becomes), "Entonces la visita queda en estado <becomes>")
            .WithExamples(new ExampleTable("status", "who", "becomes")
            {
                { VisitStatus.AwaitingHost, VisitParty.Host, VisitStatus.AwaitingVisitor },
                { VisitStatus.AwaitingVisitor, VisitParty.Visitor, VisitStatus.AwaitingHost },
            })
            .BDDfy("Proponer otras franjas pasa el turno a la otra parte");

    [TestMethod]
    public void CounterProposalReplacesSlotsAndRestartsTheDeadline() =>
        this.Given(_ => AVisitRequestedWith("2026-10-07 10:00, 2026-10-08 15:00"),
                "Dada una visita solicitada con las franjas 2026-10-07 10:00 y 2026-10-08 15:00")
            .When(_ => CounterProposes(VisitParty.Host, "2026-10-09 08:00"), "Cuando el anfitrión propone la franja 2026-10-09 08:00")
            .Then(_ => HasProposedSlots(1), "Entonces la visita tiene 1 franja propuesta")
            .And(_ => DeadlineIs("2026-10-07 09:00"), "Y el plazo para responder vence el 2026-10-07 09:00")
            .BDDfy("La contrapropuesta reemplaza las franjas y reinicia el plazo");

    [TestMethod]
    public void CounterProposalFollowsTheSlotRules() =>
        this.Given(_ => AVisitRequestedWith("2026-10-07 10:00"), "Dada una visita solicitada con la franja 2026-10-07 10:00")
            .When(_ => CounterProposes(VisitParty.Host, "2026-10-06 08:00"), "Cuando el anfitrión propone la franja 2026-10-06 08:00")
            .Then(_ => IsRejectedWith("Cada franja debe empezar en al menos 24 horas."),
                "Entonces se rechaza: Cada franja debe empezar en al menos 24 horas.")
            .BDDfy("La contrapropuesta cumple las reglas de las franjas");

    [TestMethod]
    public void OnlyThePartyWithTheTurnCanAnswer() =>
        this.Given(_ => AVisitInStatus(Status), "Dada una visita en estado <status>")
            .When(_ => Schedules(Who, VisitFactory.Slot), "Cuando <who> acepta la franja propuesta")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("status", "who", "result")
            {
                { VisitStatus.AwaitingHost, VisitParty.Host, Accepted },
                { VisitStatus.AwaitingHost, VisitParty.Visitor, "No es tu turno de responder a esta visita." },
                { VisitStatus.AwaitingVisitor, VisitParty.Visitor, Accepted },
                { VisitStatus.AwaitingVisitor, VisitParty.Host, "No es tu turno de responder a esta visita." },
            })
            .BDDfy("Solo responde quien tiene el turno");

    [TestMethod]
    public void OnlyAProposedSlotCanBeAccepted() =>
        this.Given(_ => AVisitRequestedWith("2026-10-07 10:00"), "Dada una visita solicitada con la franja 2026-10-07 10:00")
            .When(_ => Schedules(VisitParty.Host, "2026-10-09 08:00"), "Cuando el anfitrión acepta la franja 2026-10-09 08:00")
            .Then(_ => IsRejectedWith("La franja elegida no está entre las propuestas."),
                "Entonces se rechaza: La franja elegida no está entre las propuestas.")
            .BDDfy("Solo se acepta una franja propuesta");

    [TestMethod]
    public void NoAnswerIsAllowedAfterTheDeadline() =>
        this.Given(_ => AVisitRequestedWith("2026-10-07 10:00"),
                "Dada una visita solicitada con la franja 2026-10-07 10:00, cuyo plazo vence el 2026-10-06 10:00")
            .And(_ => ItIs(Moment), "Y son las <moment>")
            .When(_ => Schedules(VisitParty.Host, "2026-10-07 10:00"), "Cuando el anfitrión acepta la franja")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("moment", "result")
            {
                { "2026-10-06 09:59", Accepted },
                { "2026-10-06 10:00", "El plazo para responder a esta visita ya venció." },
            })
            .BDDfy("No se responde después del plazo");

    [TestMethod]
    public void AVisitOutOfNegotiationTakesNoAnswers() =>
        this.Given(_ => AVisitInStatus(Status), "Dada una visita en estado <status>")
            .When(_ => CounterProposes(VisitParty.Host, "2026-10-09 08:00"), "Cuando el anfitrión propone otra franja")
            .Then(_ => IsRejectedWith("La visita ya no está en negociación."),
                "Entonces se rechaza: La visita ya no está en negociación.")
            .WithExamples(new ExampleTable("status")
            {
                { VisitStatus.Scheduled },
                { VisitStatus.Completed },
                { VisitStatus.NoShow },
                { VisitStatus.Cancelled },
                { VisitStatus.Expired },
            })
            .BDDfy("Una visita fuera de negociación no admite respuestas");

    private void AVisitRequestedWith(string slots) => _visit = VisitFactory.Requested(slots);

    private void AVisitInStatus(VisitStatus status) => _visit = VisitFactory.InStatus(status);

    private void Schedules(VisitParty who, string slot) => Try(() => _visit.Schedule(who, Clock.Slot(slot), Now));

    private void CounterProposes(VisitParty who, string slots)
        => Try(() => _visit.CounterPropose(who, Clock.Slots(slots), Now));

    private void StatusIs(VisitStatus expected) => Assert.AreEqual(expected, _visit.Status);

    private void IsScheduledFor(string slot) => Assert.AreEqual(Clock.Slot(slot), _visit.ScheduledSlot);

    private void HasProposedSlots(int count) => Assert.HasCount(count, _visit.ProposedSlots);

    private void DeadlineIs(string moment) => Assert.AreEqual(Clock.At(moment), _visit.RespondBy);

    private void HasNoProposedSlotsNorDeadline()
    {
        Assert.IsEmpty(_visit.ProposedSlots);
        Assert.IsNull(_visit.RespondBy);
    }
}
