using NotificationLog.RentalService.Domain.Visits;
using NotificationLog.RentalService.Domain.Visits.Enums;

namespace NotificationLog.RentalService.UnitTests.Domain.Visits;

[TestClass]
[TestCategory("Domain-Visits")]
public class VisitExpirationTests : DomainScenario
{
    private Visit _visit = null!;

    private VisitStatus Status { get; set; }
    private VisitStatus Becomes { get; set; }
    private string Moment { get; set; } = string.Empty;

    [TestMethod]
    public void ANegotiatingVisitExpiresWhenTheDeadlinePasses() =>
        this.Given(_ => AVisitInStatus(Status), "Dada una visita en estado <status>, cuyo plazo para responder vence el 2026-10-06 10:00")
            .And(_ => ItIs(Moment), "Y son las <moment>")
            .When(_ => SystemChecksTheDeadline(), "Cuando el sistema revisa el plazo")
            .Then(_ => StatusIs(Becomes), "Entonces la visita queda en estado <becomes>")
            .WithExamples(new ExampleTable("status", "moment", "becomes")
            {
                { VisitStatus.AwaitingHost, "2026-10-06 09:59", VisitStatus.AwaitingHost },
                { VisitStatus.AwaitingHost, "2026-10-06 10:00", VisitStatus.Expired },
                { VisitStatus.AwaitingVisitor, "2026-10-06 09:59", VisitStatus.AwaitingVisitor },
                { VisitStatus.AwaitingVisitor, "2026-10-06 10:00", VisitStatus.Expired },
                { VisitStatus.Scheduled, "2026-10-08 10:00", VisitStatus.Scheduled },
                { VisitStatus.Cancelled, "2026-10-08 10:00", VisitStatus.Cancelled },
            })
            .BDDfy("Una visita en negociación vence al cumplirse el plazo para responder");

    [TestMethod]
    public void AnExpiredVisitKeepsNoSlotsNorDeadline() =>
        this.Given(_ => AVisitInStatus(VisitStatus.AwaitingHost), "Dada una visita esperando al anfitrión")
            .And(_ => ItIs("2026-10-06 10:00"), "Y son las 2026-10-06 10:00")
            .When(_ => SystemChecksTheDeadline(), "Cuando el sistema revisa el plazo")
            .Then(_ => HasNoProposedSlotsNorDeadline(), "Entonces la visita ya no tiene franjas propuestas ni plazo para responder")
            .BDDfy("Una visita vencida no conserva franjas ni plazo");

    private void AVisitInStatus(VisitStatus status) => _visit = VisitFactory.InStatus(status);

    private void SystemChecksTheDeadline() => _visit.Expire(Now);

    private void StatusIs(VisitStatus expected) => Assert.AreEqual(expected, _visit.Status);

    private void HasNoProposedSlotsNorDeadline()
    {
        Assert.IsEmpty(_visit.ProposedSlots);
        Assert.IsNull(_visit.RespondBy);
    }
}
