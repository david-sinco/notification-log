using NotificationLog.RentalService.Domain.Common.ValueObjects;
using NotificationLog.RentalService.Domain.Owners;
using NotificationLog.RentalService.Domain.Owners.Events;
using NotificationLog.RentalService.Domain.Owners.ValueObjects;

namespace NotificationLog.RentalService.UnitTests.Domain.Owners;

[TestClass]
public class OwnerRegistrationTests : DomainScenario
{
    private static readonly Guid User = Guid.NewGuid();

    private Owner _owner = null!;

    [TestMethod]
    public void AnOwnerRegistersWithNameAndContact() =>
        this.When(_ => AnOwnerIsRegistered(Guid.NewGuid(), "Inmobiliaria Andes"),
                "Cuando se registra a Inmobiliaria Andes con correo andes@example.com y teléfono 6011234567")
            .Then(_ => NameIs("Inmobiliaria Andes"), "Entonces se llama Inmobiliaria Andes")
            .And(_ => ContactIs("andes@example.com", "+576011234567"), "Y su contacto es andes@example.com y +576011234567")
            .And(_ => WasRegisteredByTheUser(), "Y queda registrado qué usuario lo creó")
            .And(_ => TheRegistrationIsRecorded(), "Y se registra el alta del propietario")
            .BDDfy("Un propietario se registra con su nombre y sus datos de contacto");

    [TestMethod]
    public void AnOwnerNeedsAnIdentifier() =>
        this.When(_ => AnOwnerIsRegistered(Guid.Empty, "Inmobiliaria Andes"), "Cuando se registra un propietario sin identificador")
            .Then(_ => IsRejectedWith("El identificador del propietario es obligatorio."),
                "Entonces se rechaza: El identificador del propietario es obligatorio.")
            .BDDfy("Un propietario necesita identificador");

    [TestMethod]
    public void AnOwnerNeedsAName() =>
        this.When(_ => AnOwnerIsRegistered(Guid.NewGuid(), " "), "Cuando se registra un propietario sin nombre")
            .Then(_ => IsRejectedWith("El nombre del propietario es obligatorio."),
                "Entonces se rechaza: El nombre del propietario es obligatorio.")
            .BDDfy("Un propietario necesita nombre");

    private void AnOwnerIsRegistered(Guid id, string name)
        => Try(() => _owner = Owner.Register(
            id,
            User,
            null,
            OwnerName.Create(name),
            ContactInfo.Create("andes@example.com", "6011234567")));

    private void NameIs(string name) => Assert.AreEqual(name, _owner.Name?.Value);

    private void ContactIs(string email, string phone)
    {
        Assert.AreEqual(email, _owner.Contact?.Email);
        Assert.AreEqual(phone, _owner.Contact?.Phone);
    }

    private void WasRegisteredByTheUser() => Assert.AreEqual(User, _owner.CreatedBy);

    private void TheRegistrationIsRecorded()
        => Assert.IsInstanceOfType<OwnerRegistered>(_owner.DomainEvents.Single());
}
