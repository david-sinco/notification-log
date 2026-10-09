using NotificationLog.RentalService.Application.Visits.Commands.MarkVisitNoShow;
using NotificationLog.RentalService.Domain.Listings.Enums;
using NotificationLog.RentalService.Domain.Visits;
using NotificationLog.RentalService.Domain.Visits.Enums;

namespace NotificationLog.RentalService.UnitTests.Application.Visits.Commands.MarkVisitNoShow;

[TestClass]
[TestCategory("Application-Visits")]
public class MarkVisitNoShowHandlerTests : ApplicationScenario
{
    private Visit _visit = null!;

    [TestMethod]
    public void TheHostMarksItNoShowAndTheVisitorIsNotified() =>
        this.Given(_ => AScheduledVisit(), "Dada una visita agendada para el 2026-10-07 10:00")
            .And(_ => HostSignsIn(), "Y que el anfitrión inicia sesión")
            .And(_ => ItIs("2026-10-07 11:30"), "Y son las 2026-10-07 11:30")
            .When(_ => MarksItNoShow(), "Cuando marca que el visitante no asistió")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => StatusIs(VisitStatus.NoShow), "Y la visita queda como inasistencia")
            .And(_ => IsSaved(_visit), "Y se guarda el cambio")
            .And(_ => IsNotified(VisitNotificationKeys.NoShow, VisitFactory.Visitor), "Y se notifica al visitante")
            .BDDfy("El anfitrión marca la inasistencia y se notifica al visitante");

    [TestMethod]
    public void TheClosingTimeComesFromTheClock() =>
        this.Given(_ => AScheduledVisit(), "Dada una visita agendada para el 2026-10-07 10:00")
            .And(_ => HostSignsIn(), "Y que el anfitrión inicia sesión")
            .And(_ => ItIs("2026-10-07 09:59"), "Y son las 2026-10-07 09:59")
            .When(_ => MarksItNoShow(), "Cuando marca que el visitante no asistió")
            .Then(_ => IsRejectedWith("La visita todavía no ha empezado."), "Entonces se rechaza: La visita todavía no ha empezado.")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .And(_ => NobodyIsNotified(), "Y no se notifica a nadie")
            .BDDfy("La hora de cierre se compara con la hora actual");

    [TestMethod]
    public void OnlyTheHostCanCloseTheVisit() =>
        this.Given(_ => AScheduledVisit(), "Dada una visita agendada para el 2026-10-07 10:00")
            .And(_ => VisitorSignsIn(), "Y que el visitante inicia sesión")
            .And(_ => ItIs("2026-10-07 11:30"), "Y son las 2026-10-07 11:30")
            .When(_ => MarksItNoShow(), "Cuando marca que el visitante no asistió")
            .Then(_ => IsRejectedWith("Solo el anfitrión puede cerrar la visita."), "Entonces se rechaza: Solo el anfitrión puede cerrar la visita.")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .BDDfy("Solo el anfitrión puede cerrar la visita");

    [TestMethod]
    public void AThirdPartyCannotCloseTheVisit() =>
        this.Given(_ => AScheduledVisit(), "Dada una visita agendada para el 2026-10-07 10:00")
            .And(_ => AThirdPartySignsIn(), "Y que un tercero inicia sesión")
            .When(_ => MarksItNoShow(), "Cuando marca que el visitante no asistió")
            .Then(_ => IsForbidden(), "Entonces se rechaza por falta de permiso")
            .BDDfy("Un tercero no puede cerrar la visita");

    [TestMethod]
    public void AMissingVisitIsNotFound() =>
        this.Given(_ => AMissingVisit(), "Dada una visita que no existe")
            .And(_ => HostSignsIn(), "Y que un anfitrión inicia sesión")
            .When(_ => MarksItNoShow(), "Cuando marca que el visitante no asistió")
            .Then(_ => IsNotFound(), "Entonces la visita no se encuentra")
            .BDDfy("No se puede cerrar una visita que no existe");

    private void AScheduledVisit()
    {
        Exists(_visit = VisitFactory.InStatus(VisitStatus.Scheduled));
        AnyListingIs(ListingFactory.InStatus(ListingStatus.Published));
    }

    private void AMissingVisit() => _visit = VisitFactory.InStatus(VisitStatus.Scheduled);

    private Task MarksItNoShow()
        => TryAsync(() => Handler<MarkVisitNoShowHandler>().HandleAsync(new MarkVisitNoShowCommand(_visit.Id), User, CancellationToken.None));

    private void StatusIs(VisitStatus expected) => Assert.AreEqual(expected, _visit.Status);
}
