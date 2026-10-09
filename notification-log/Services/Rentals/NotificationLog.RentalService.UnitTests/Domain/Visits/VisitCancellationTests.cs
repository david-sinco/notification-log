using NotificationLog.RentalService.Domain.Visits;
using NotificationLog.RentalService.Domain.Visits.Enums;
using NotificationLog.RentalService.Domain.Visits.Events;

namespace NotificationLog.RentalService.UnitTests.Domain.Visits;

[TestClass]
[TestCategory("Domain-Visits")]
public class VisitCancellationTests : DomainScenario
{
    private Visit _visit = null!;

    private VisitStatus Status { get; set; }
    private VisitParty Who { get; set; }
    private string Moment { get; set; } = string.Empty;
    private bool Late { get; set; }
    private int Length { get; set; }
    private string Result { get; set; } = string.Empty;

    [TestMethod]
    public void OnlyNegotiatingOrScheduledVisitsCanBeCancelled() =>
        this.Given(_ => AVisitInStatus(Status), "Dada una visita en estado <status>")
            .When(_ => Cancels(VisitParty.Host), "Cuando el anfitrión la cancela")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("status", "result")
            {
                { VisitStatus.AwaitingHost, Accepted },
                { VisitStatus.AwaitingVisitor, Accepted },
                { VisitStatus.Scheduled, Accepted },
                { VisitStatus.Expired, "Solo se puede cancelar una visita en negociación o agendada." },
                { VisitStatus.Completed, "Solo se puede cancelar una visita en negociación o agendada." },
                { VisitStatus.NoShow, "Solo se puede cancelar una visita en negociación o agendada." },
            })
            .BDDfy("Solo se cancela una visita en negociación o agendada");

    [TestMethod]
    public void CancellationRecordsWhoCancelledAndWhy() =>
        this.Given(_ => AVisitInStatus(VisitStatus.AwaitingHost), "Dada una visita esperando al anfitrión")
            .When(_ => CancelsBecause(VisitParty.Host, "  El inmueble ya no está disponible  "),
                "Cuando el anfitrión la cancela con el motivo «  El inmueble ya no está disponible  »")
            .Then(_ => StatusIs(VisitStatus.Cancelled), "Entonces la visita queda cancelada")
            .And(_ => WasCancelledBy(VisitParty.Host, "El inmueble ya no está disponible"),
                "Y queda registrado que la canceló el anfitrión con el motivo sin espacios sobrantes")
            .And(_ => HasNoProposedSlotsNorDeadline(), "Y ya no tiene franjas propuestas ni plazo para responder")
            .BDDfy("La cancelación registra quién cancela y el motivo");

    [TestMethod]
    public void CancellingACancelledVisitHasNoEffect() =>
        this.Given(_ => AVisitInStatus(VisitStatus.Cancelled), "Dada una visita ya cancelada")
            .When(_ => Cancels(VisitParty.Visitor), "Cuando el visitante la cancela de nuevo")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => NothingIsRecorded(), "Y no se registra otra cancelación")
            .BDDfy("Cancelar una visita ya cancelada no tiene efecto");

    [TestMethod]
    public void AReasonOfUpTo500CharactersIsRequired() =>
        this.Given(_ => AVisitInStatus(VisitStatus.AwaitingHost), "Dada una visita esperando al anfitrión")
            .When(_ => CancelsWithReasonOfLength(Length), "Cuando el anfitrión la cancela con un motivo de <length> caracteres")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("length", "result")
            {
                { 0, "Hay que indicar el motivo." },
                { 1, Accepted },
                { 500, Accepted },
                { 501, "El motivo no puede superar 500 caracteres." },
            })
            .BDDfy("Hay que indicar un motivo de hasta 500 caracteres");

    [TestMethod]
    public void OnlyTheVisitorCancellingWithin12HoursIsLate() =>
        this.Given(_ => AVisitScheduledForOctober7At10(), "Dada una visita agendada para el 2026-10-07 10:00")
            .And(_ => ItIs(Moment), "Y son las <moment>")
            .When(_ => Cancels(Who), "Cuando <who> la cancela")
            .Then(_ => CancellationIsLate(Late), "Entonces la cancelación es tardía: <late>")
            .WithExamples(new ExampleTable("who", "moment", "late")
            {
                { VisitParty.Visitor, "2026-10-06 21:00", false },
                { VisitParty.Visitor, "2026-10-06 22:00", false },
                { VisitParty.Visitor, "2026-10-06 22:01", true },
                { VisitParty.Visitor, "2026-10-07 08:00", true },
                { VisitParty.Host, "2026-10-07 08:00", false },
            })
            .BDDfy("Solo es tardía la cancelación del visitante a menos de 12 horas de la visita");

    [TestMethod]
    public void CancellingDuringNegotiationIsNeverLate() =>
        this.Given(_ => AVisitInStatus(VisitStatus.AwaitingHost), "Dada una visita esperando al anfitrión")
            .When(_ => Cancels(VisitParty.Visitor), "Cuando el visitante la cancela")
            .Then(_ => CancellationIsLate(false), "Entonces la cancelación no es tardía")
            .BDDfy("Una cancelación en negociación nunca es tardía");

    [TestMethod]
    public void AStartedVisitCannotBeCancelled() =>
        this.Given(_ => AVisitScheduledForOctober7At10(), "Dada una visita agendada para el 2026-10-07 10:00")
            .And(_ => ItIs(Moment), "Y son las <moment>")
            .When(_ => Cancels(VisitParty.Visitor), "Cuando el visitante la cancela")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("moment", "result")
            {
                { "2026-10-07 09:59", Accepted },
                { "2026-10-07 10:00", "La visita ya empezó; no se puede cancelar." },
            })
            .BDDfy("No se cancela una visita que ya empezó");

    private void AVisitInStatus(VisitStatus status) => _visit = VisitFactory.InStatus(status);

    private void AVisitScheduledForOctober7At10() => _visit = VisitFactory.InStatus(VisitStatus.Scheduled);

    private void Cancels(VisitParty who) => CancelsBecause(who, "No puedo asistir");

    private void CancelsWithReasonOfLength(int length) => CancelsBecause(VisitParty.Host, new string('x', length));

    private void CancelsBecause(VisitParty who, string reason) => Try(() => _visit.Cancel(who, reason, Now));

    private void StatusIs(VisitStatus expected) => Assert.AreEqual(expected, _visit.Status);

    private void WasCancelledBy(VisitParty who, string reason)
    {
        Assert.AreEqual(who, _visit.CancelledBy);
        Assert.AreEqual(reason, Cancellation().Reason);
    }

    private void CancellationIsLate(bool late)
    {
        Assert.AreEqual(late, Cancellation().IsLate);
        Assert.AreEqual(late, _visit.IsLateCancellation);
    }

    private void NothingIsRecorded() => HasNoEffect(_visit);

    private void HasNoProposedSlotsNorDeadline()
    {
        Assert.IsEmpty(_visit.ProposedSlots);
        Assert.IsNull(_visit.RespondBy);
    }

    private VisitCancelled Cancellation() => _visit.DomainEvents.OfType<VisitCancelled>().Single();
}
