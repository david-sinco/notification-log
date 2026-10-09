using NotificationLog.RentalService.Domain.Visitors;
using NotificationLog.RentalService.Domain.Visitors.Events;

namespace NotificationLog.RentalService.UnitTests.Domain.Visitors;

[TestClass]
[TestCategory("Domain-Visitors")]
public class VisitorTests : DomainScenario
{
    private Visitor _visitor = null!;

    [TestMethod]
    public void AVisitorIsRegisteredWithACopyOfTheUserData() =>
        this.When(_ => AVisitorIsRegistered(), "Cuando se registra un visitante con los datos del usuario")
            .Then(_ => HasData("Víctor Rojas Peña", "victor@example.com", "+573109876543"),
                "Entonces el visitante guarda su nombre, correo y teléfono")
            .BDDfy("Un visitante se registra con una copia de los datos del usuario");

    [TestMethod]
    public void OnlyTheDataThatComesIsUpdated() =>
        this.Given(_ => AVisitorIsRegistered(), "Dado un visitante registrado")
            .When(_ => Updates("", "nuevo@example.com", ""), "Cuando llega solo un correo nuevo")
            .Then(_ => HasData("Víctor Rojas Peña", "nuevo@example.com", "+573109876543"),
                "Entonces cambia el correo y se conservan el nombre y el teléfono")
            .And(_ => AnUpdateIsRaised(), "Y se registra la actualización")
            .BDDfy("La actualización solo cambia los datos que llegan");

    [TestMethod]
    public void AnUpdateWithoutChangesIsRejected() =>
        this.Given(_ => AVisitorIsRegistered(), "Dado un visitante registrado")
            .When(_ => Updates("Víctor Rojas Peña", "victor@example.com", ""), "Cuando llegan los mismos datos")
            .Then(_ => IsRejectedWith($"Los datos del visitante '{_visitor.Id}' no cambiaron."),
                "Entonces se rechaza porque los datos no cambiaron")
            .And(_ => NoUpdateIsRaised(), "Y no se registra ninguna actualización")
            .BDDfy("Una actualización sin cambios no genera eventos");

    private void AVisitorIsRegistered()
    {
        _visitor = Visitor.Register(Guid.NewGuid(), "Víctor Rojas Peña", "victor@example.com", "+573109876543");
        _visitor.ClearDomainEvents();
    }

    private void Updates(string name, string email, string phone) => Try(() => _visitor.Update(name, email, phone));

    private void HasData(string name, string email, string phone)
    {
        Assert.AreEqual(name, _visitor.Name);
        Assert.AreEqual(email, _visitor.Email);
        Assert.AreEqual(phone, _visitor.Phone);
    }

    private void AnUpdateIsRaised() => Assert.IsInstanceOfType<VisitorUpdated>(_visitor.DomainEvents.Single());

    private void NoUpdateIsRaised() => Assert.IsEmpty(_visitor.DomainEvents);
}
