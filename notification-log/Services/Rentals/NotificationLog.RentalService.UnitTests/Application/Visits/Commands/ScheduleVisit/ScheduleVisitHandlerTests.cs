using NotificationLog.RentalService.Application.Visits.Commands.ScheduleVisit;
using NotificationLog.RentalService.Domain.Listings.Enums;
using NotificationLog.RentalService.Domain.Visits;
using NotificationLog.RentalService.Domain.Visits.Enums;
using NSubstitute;

namespace NotificationLog.RentalService.UnitTests.Application.Visits.Commands.ScheduleVisit;

[TestClass]
[TestCategory("Application-Visits")]
public class ScheduleVisitHandlerTests : ApplicationScenario
{
    private Visit _visit = null!;

    [TestMethod]
    public void TheHostSchedulesAndTheVisitorIsNotified() =>
        this.Given(_ => AVisitInStatus(VisitStatus.AwaitingHost), "Dada una visita esperando al anfitrión con la franja 2026-10-07 10:00")
            .And(_ => HostSignsIn(), "Y que el anfitrión inicia sesión")
            .When(_ => SchedulesFor(Clock.At(VisitFactory.Slot)), "Cuando acepta la franja 2026-10-07 10:00")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => StatusIs(VisitStatus.Scheduled), "Y la visita queda agendada")
            .And(_ => IsSaved(_visit), "Y se guarda el cambio")
            .And(_ => IsNotifiedOfTheSchedule(VisitFactory.Visitor), "Y se notifica al visitante la visita agendada con su hora")
            .BDDfy("El anfitrión agenda la visita y se notifica al visitante");

    [TestMethod]
    public void TheVisitorSchedulesAndTheHostIsNotified() =>
        this.Given(_ => AVisitInStatus(VisitStatus.AwaitingVisitor), "Dada una visita esperando al visitante con la franja 2026-10-07 10:00")
            .And(_ => VisitorSignsIn(), "Y que el visitante inicia sesión")
            .When(_ => SchedulesFor(Clock.At(VisitFactory.Slot)), "Cuando acepta la franja 2026-10-07 10:00")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => IsNotifiedOfTheSchedule(VisitFactory.Host), "Y se notifica al anfitrión la visita agendada con su hora")
            .BDDfy("El visitante agenda la visita y se notifica al anfitrión");

    [TestMethod]
    public void AThirdPartyCannotAnswer() =>
        this.Given(_ => AVisitInStatus(VisitStatus.AwaitingHost), "Dada una visita esperando al anfitrión")
            .And(_ => AThirdPartySignsIn(), "Y que un tercero inicia sesión")
            .When(_ => SchedulesFor(Clock.At(VisitFactory.Slot)), "Cuando acepta la franja")
            .Then(_ => IsForbidden(), "Entonces se rechaza por falta de permiso")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .And(_ => NobodyIsNotified(), "Y no se notifica a nadie")
            .BDDfy("Un tercero no puede agendar la visita");

    [TestMethod]
    public void TheSlotIsRequired() =>
        this.Given(_ => AVisitInStatus(VisitStatus.AwaitingHost), "Dada una visita esperando al anfitrión")
            .And(_ => HostSignsIn(), "Y que el anfitrión inicia sesión")
            .When(_ => SchedulesFor(default), "Cuando agenda sin indicar la franja")
            .Then(_ => FailsValidationOn("StartsAt"), "Entonces la validación falla en la franja")
            .BDDfy("Agendar exige indicar la franja");

    [TestMethod]
    public void AMissingVisitIsNotFound() =>
        this.Given(_ => AMissingVisit(), "Dada una visita que no existe")
            .And(_ => HostSignsIn(), "Y que un anfitrión inicia sesión")
            .When(_ => SchedulesFor(Clock.At(VisitFactory.Slot)), "Cuando acepta una franja")
            .Then(_ => IsNotFound(), "Entonces la visita no se encuentra")
            .BDDfy("No se puede agendar una visita que no existe");

    private void AVisitInStatus(VisitStatus status)
    {
        Exists(_visit = VisitFactory.InStatus(status));
        AnyListingIs(ListingFactory.InStatus(ListingStatus.Published));
    }

    private void AMissingVisit() => _visit = VisitFactory.Requested();

    private Task SchedulesFor(DateTimeOffset startsAt)
        => TryAsync(() => Handler<ScheduleVisitHandler>()
            .HandleAsync(new ScheduleVisitCommand(_visit.Id, startsAt), User, CancellationToken.None));

    private void StatusIs(VisitStatus expected) => Assert.AreEqual(expected, _visit.Status);

    private void IsNotifiedOfTheSchedule(Guid recipient)
        => Notifications.Received(1).NotifyAsync(
            VisitNotificationKeys.Scheduled,
            recipient,
            Arg.Is<IReadOnlyDictionary<string, string>>(data => data["starts_at"] == "2026-10-07 10:00"),
            Arg.Any<CancellationToken>());
}
