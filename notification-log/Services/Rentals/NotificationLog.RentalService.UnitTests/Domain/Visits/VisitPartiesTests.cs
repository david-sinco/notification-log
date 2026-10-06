using NotificationLog.RentalService.Domain.Visits;
using NotificationLog.RentalService.Domain.Visits.Enums;

namespace NotificationLog.RentalService.UnitTests.Domain.Visits;

[TestClass]
public class VisitPartiesTests : DomainScenario
{
    private Visit _visit = null!;

    [TestMethod]
    public void HostAndVisitorAreThePartiesOfTheVisit() =>
        this.Given(_ => ARequestedVisit(), "Dada una visita solicitada")
            .Then(_ => HostIsParty(VisitParty.Host), "Entonces el anfitrión participa como anfitrión")
            .And(_ => VisitorIsParty(VisitParty.Visitor), "Y el visitante participa como visitante")
            .BDDfy("El anfitrión y el visitante son las partes de la visita");

    [TestMethod]
    public void EachPartyHasTheOtherAsCounterpart() =>
        this.Given(_ => ARequestedVisit(), "Dada una visita solicitada")
            .Then(_ => CounterpartOfHostIsVisitor(), "Entonces la contraparte del anfitrión es el visitante")
            .And(_ => CounterpartOfVisitorIsHost(), "Y la contraparte del visitante es el anfitrión")
            .BDDfy("Cada parte tiene a la otra como contraparte");

    [TestMethod]
    public void AThirdPartyDoesNotTakePartInTheVisit() =>
        this.Given(_ => ARequestedVisit(), "Dada una visita solicitada")
            .When(_ => AThirdPartyTriesToTakePart(), "Cuando un tercero intenta participar")
            .Then(_ => IsRejectedWith("No participas en esta visita."), "Entonces se rechaza: No participas en esta visita.")
            .BDDfy("Un tercero no participa en la visita");

    private void ARequestedVisit() => _visit = VisitFactory.Requested();

    private void AThirdPartyTriesToTakePart() => Try(() => _visit.PartyOf(Guid.NewGuid()));

    private void HostIsParty(VisitParty party) => Assert.AreEqual(party, _visit.PartyOf(VisitFactory.Host));

    private void VisitorIsParty(VisitParty party) => Assert.AreEqual(party, _visit.PartyOf(VisitFactory.Visitor));

    private void CounterpartOfHostIsVisitor()
        => Assert.AreEqual(VisitFactory.Visitor, _visit.CounterpartOf(VisitParty.Host));

    private void CounterpartOfVisitorIsHost()
        => Assert.AreEqual(VisitFactory.Host, _visit.CounterpartOf(VisitParty.Visitor));
}
