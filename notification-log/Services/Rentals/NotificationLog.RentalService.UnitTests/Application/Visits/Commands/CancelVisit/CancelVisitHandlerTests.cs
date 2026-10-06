using NotificationLog.RentalService.Application.Visits.Commands.CancelVisit;
using NotificationLog.RentalService.Domain.Listings.Enums;
using NotificationLog.RentalService.Domain.Visits;
using NotificationLog.RentalService.Domain.Visits.Enums;
using NSubstitute;

namespace NotificationLog.RentalService.UnitTests.Application.Visits.Commands.CancelVisit;

[TestClass]
public class CancelVisitHandlerTests : ApplicationScenario
{
    private Visit _visit = null!;

    [TestMethod]
    public void TheHostCancelsAndTheVisitorIsNotified() =>
        this.Given(_ => AVisitInStatus(VisitStatus.AwaitingHost), "Dada una visita en negociación")
            .And(_ => HostSignsIn(), "Y que el anfitrión inicia sesión")
            .When(_ => CancelsBecause("  El inmueble ya no está disponible  "), "Cuando la cancela con el motivo «  El inmueble ya no está disponible  »")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => WasCancelledBy(VisitParty.Host), "Y la visita queda cancelada por el anfitrión")
            .And(_ => IsSaved(_visit), "Y se guarda el cambio")
            .And(_ => IsNotifiedOfTheCancellation(VisitFactory.Visitor), "Y se notifica al visitante la cancelación con el motivo sin espacios sobrantes")
            .BDDfy("El anfitrión cancela la visita y se notifica al visitante");

    [TestMethod]
    public void TheVisitorCancelsAndTheHostIsNotified() =>
        this.Given(_ => AVisitInStatus(VisitStatus.AwaitingHost), "Dada una visita en negociación")
            .And(_ => VisitorSignsIn(), "Y que el visitante inicia sesión")
            .When(_ => CancelsBecause("El inmueble ya no está disponible"), "Cuando la cancela")
            .Then(_ => WasCancelledBy(VisitParty.Visitor), "Entonces la visita queda cancelada por el visitante")
            .And(_ => IsNotifiedOfTheCancellation(VisitFactory.Host), "Y se notifica al anfitrión la cancelación")
            .BDDfy("El visitante cancela la visita y se notifica al anfitrión");

    [TestMethod]
    public void CancellingACancelledVisitSavesAndNotifiesNothing() =>
        this.Given(_ => AVisitInStatus(VisitStatus.Cancelled), "Dada una visita ya cancelada")
            .And(_ => VisitorSignsIn(), "Y que el visitante inicia sesión")
            .When(_ => CancelsBecause("Ya no me interesa"), "Cuando la cancela de nuevo")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .And(_ => NobodyIsNotified(), "Y no se notifica a nadie")
            .BDDfy("Cancelar una visita ya cancelada no guarda ni notifica nada");

    [TestMethod]
    public void AThirdPartyCannotCancel() =>
        this.Given(_ => AVisitInStatus(VisitStatus.AwaitingHost), "Dada una visita en negociación")
            .And(_ => AThirdPartySignsIn(), "Y que un tercero inicia sesión")
            .When(_ => CancelsBecause("Ya no me interesa"), "Cuando la cancela")
            .Then(_ => IsForbidden(), "Entonces se rechaza por falta de permiso")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .BDDfy("Un tercero no puede cancelar la visita");

    [TestMethod]
    public void AReasonIsRequired() =>
        this.Given(_ => AVisitInStatus(VisitStatus.AwaitingHost), "Dada una visita en negociación")
            .And(_ => HostSignsIn(), "Y que el anfitrión inicia sesión")
            .When(_ => CancelsBecause(string.Empty), "Cuando la cancela sin motivo")
            .Then(_ => FailsValidationOn("Reason"), "Entonces la validación falla en el motivo")
            .BDDfy("Cancelar exige un motivo");

    [TestMethod]
    public void AMissingVisitIsNotFound() =>
        this.Given(_ => AMissingVisit(), "Dada una visita que no existe")
            .And(_ => HostSignsIn(), "Y que un anfitrión inicia sesión")
            .When(_ => CancelsBecause("Ya no me interesa"), "Cuando la cancela")
            .Then(_ => IsNotFound(), "Entonces la visita no se encuentra")
            .BDDfy("No se puede cancelar una visita que no existe");

    private void AVisitInStatus(VisitStatus status)
    {
        Exists(_visit = VisitFactory.InStatus(status));
        AnyListingIs(ListingFactory.InStatus(ListingStatus.Published));
    }

    private void AMissingVisit() => _visit = VisitFactory.Requested();

    private Task CancelsBecause(string reason)
        => TryAsync(() => Handler<CancelVisitHandler>()
            .HandleAsync(new CancelVisitCommand(_visit.Id, reason), User, CancellationToken.None));

    private void WasCancelledBy(VisitParty who)
    {
        Assert.AreEqual(VisitStatus.Cancelled, _visit.Status);
        Assert.AreEqual(who, _visit.CancelledBy);
    }

    private void IsNotifiedOfTheCancellation(Guid recipient)
        => Notifications.Received(1).NotifyAsync(
            VisitNotificationKeys.Cancelled,
            recipient,
            Arg.Is<IReadOnlyDictionary<string, string>>(data => data["reason"] == "El inmueble ya no está disponible"),
            Arg.Any<CancellationToken>());
}
