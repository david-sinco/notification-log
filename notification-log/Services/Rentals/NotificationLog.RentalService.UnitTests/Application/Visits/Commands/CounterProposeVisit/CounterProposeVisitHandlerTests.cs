using NotificationLog.RentalService.Application.Visits.Commands.CounterProposeVisit;
using NotificationLog.RentalService.Domain.Listings.Enums;
using NotificationLog.RentalService.Domain.Visits;
using NotificationLog.RentalService.Domain.Visits.Enums;
using NSubstitute;

namespace NotificationLog.RentalService.UnitTests.Application.Visits.Commands.CounterProposeVisit;

[TestClass]
[TestCategory("Application-Visits")]
public class CounterProposeVisitHandlerTests : ApplicationScenario
{
    private Visit _visit = null!;

    [TestMethod]
    public void TheHostCounterProposesAndTheVisitorIsNotified() =>
        this.Given(_ => ARequestedVisit(), "Dada una visita esperando al anfitrión")
            .And(_ => HostSignsIn(), "Y que el anfitrión inicia sesión")
            .When(_ => CounterProposes(1), "Cuando propone la franja 2026-10-09 08:00")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => StatusIs(VisitStatus.AwaitingVisitor), "Y la visita queda esperando al visitante")
            .And(_ => IsSaved(_visit), "Y se guarda el cambio")
            .And(_ => TheVisitorIsNotifiedWithTheSlots(), "Y se notifica al visitante la contrapropuesta con las franjas")
            .BDDfy("El anfitrión contrapropone y se notifica al visitante");

    [TestMethod]
    public void AThirdPartyCannotAnswer() =>
        this.Given(_ => ARequestedVisit(), "Dada una visita esperando al anfitrión")
            .And(_ => AThirdPartySignsIn(), "Y que un tercero inicia sesión")
            .When(_ => CounterProposes(1), "Cuando propone otra franja")
            .Then(_ => IsForbidden(), "Entonces se rechaza por falta de permiso")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .And(_ => NobodyIsNotified(), "Y no se notifica a nadie")
            .BDDfy("Un tercero no puede responder a la visita");

    [TestMethod]
    public void SlotsAreRequired() =>
        this.Given(_ => ARequestedVisit(), "Dada una visita esperando al anfitrión")
            .And(_ => HostSignsIn(), "Y que el anfitrión inicia sesión")
            .When(_ => CounterProposes(0), "Cuando contrapropone sin franjas")
            .Then(_ => FailsValidationOn("Slots"), "Entonces la validación falla en las franjas")
            .BDDfy("La contrapropuesta exige franjas");

    [TestMethod]
    public void AMissingVisitIsNotFound() =>
        this.Given(_ => AMissingVisit(), "Dada una visita que no existe")
            .And(_ => HostSignsIn(), "Y que un anfitrión inicia sesión")
            .When(_ => CounterProposes(1), "Cuando propone otra franja")
            .Then(_ => IsNotFound(), "Entonces la visita no se encuentra")
            .BDDfy("No se puede responder a una visita que no existe");

    private void ARequestedVisit()
    {
        Exists(_visit = VisitFactory.Requested());
        AnyListingIs(ListingFactory.InStatus(ListingStatus.Published));
    }

    private void AMissingVisit() => _visit = VisitFactory.Requested();

    private Task CounterProposes(int count)
    {
        var slots = Enumerable.Range(0, count).Select(i => Clock.At("2026-10-09 08:00").AddHours(2 * i)).ToList();

        return TryAsync(() => Handler<CounterProposeVisitHandler>()
            .HandleAsync(new CounterProposeVisitCommand(_visit.Id, slots), User, CancellationToken.None));
    }

    private void StatusIs(VisitStatus expected) => Assert.AreEqual(expected, _visit.Status);

    private void TheVisitorIsNotifiedWithTheSlots()
        => Notifications.Received(1).NotifyAsync(
            VisitNotificationKeys.CounterProposed,
            VisitFactory.Visitor,
            Arg.Is<IReadOnlyDictionary<string, string>>(data => data["slots"] == "2026-10-09 08:00"),
            Arg.Any<CancellationToken>());
}
