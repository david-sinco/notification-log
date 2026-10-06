using NotificationLog.RentalService.Domain.Common.Enums;
using NotificationLog.RentalService.Domain.Common.ValueObjects;
using NotificationLog.RentalService.Domain.Owners;
using NotificationLog.RentalService.Domain.Owners.Enums;
using NotificationLog.RentalService.Domain.Owners.Events;
using NotificationLog.RentalService.Domain.Owners.ValueObjects;

namespace NotificationLog.RentalService.UnitTests.Domain.Owners;

[TestClass]
public class OwnerRegistrationTests : DomainScenario
{
    private static readonly Guid User = Guid.NewGuid();

    private Owner _owner = null!;

    [TestMethod]
    public void ANaturalPersonRegistersWithNameAndDocument() =>
        this.When(_ => ANaturalPersonIsRegistered(Guid.NewGuid()),
                "Cuando se registra a Ana Gómez Rincón con cédula 52123456, correo ana@example.com y teléfono 3001234567")
            .Then(_ => TypeIs(OwnerType.Natural), "Entonces el propietario es persona natural")
            .And(_ => NameIs("Ana Gómez Rincón"), "Y se llama Ana Gómez Rincón")
            .And(_ => DocumentIs("52123456"), "Y su documento es 52123456")
            .And(_ => ContactIs("ana@example.com", "+573001234567"), "Y su contacto es ana@example.com y +573001234567")
            .And(_ => WasRegisteredByTheUser(), "Y queda registrado qué usuario lo creó")
            .And(_ => NaturalRegistrationIsRecorded(), "Y se registra el alta de persona natural")
            .BDDfy("Una persona natural se registra con su nombre y documento");

    [TestMethod]
    public void ACompanyRegistersWithLegalNameAndNit() =>
        this.When(_ => ACompanyIsRegistered(),
                "Cuando se registra a Inmobiliaria Andes con NIT 900123456-8, correo andes@example.com y teléfono 6011234567")
            .Then(_ => TypeIs(OwnerType.Company), "Entonces el propietario es persona jurídica")
            .And(_ => LegalNameIs("Inmobiliaria Andes"), "Y su razón social es Inmobiliaria Andes")
            .And(_ => NitIs("900123456-8"), "Y su NIT es 900123456-8")
            .And(_ => ContactIs("andes@example.com", "+576011234567"), "Y su contacto es andes@example.com y +576011234567")
            .And(_ => WasRegisteredByTheUser(), "Y queda registrado qué usuario lo creó")
            .And(_ => CompanyRegistrationIsRecorded(), "Y se registra el alta de persona jurídica")
            .BDDfy("Una persona jurídica se registra con su razón social y NIT");

    [TestMethod]
    public void AnOwnerNeedsAnIdentifier() =>
        this.When(_ => ANaturalPersonIsRegistered(Guid.Empty), "Cuando se registra una persona natural sin identificador")
            .Then(_ => IsRejectedWith("El identificador del propietario es obligatorio."),
                "Entonces se rechaza: El identificador del propietario es obligatorio.")
            .BDDfy("Un propietario necesita identificador");

    private void ANaturalPersonIsRegistered(Guid id)
        => Try(() => _owner = Owner.RegisterNatural(
            id,
            User,
            PersonName.Create("Ana", "Gómez Rincón"),
            IdentityDocument.Create(DocumentType.CitizenshipCard, "52123456"),
            ContactInfo.Create("ana@example.com", "3001234567")));

    private void ACompanyIsRegistered()
        => Try(() => _owner = Owner.RegisterCompany(
            Guid.NewGuid(),
            User,
            LegalName.Create("Inmobiliaria Andes"),
            Nit.Create("900123456-8"),
            ContactInfo.Create("andes@example.com", "6011234567")));

    private void TypeIs(OwnerType expected) => Assert.AreEqual(expected, _owner.Type);

    private void NameIs(string name) => Assert.AreEqual(name, _owner.Name?.ToString());

    private void DocumentIs(string number) => Assert.AreEqual(number, _owner.Document?.Number);

    private void LegalNameIs(string legalName) => Assert.AreEqual(legalName, _owner.LegalName?.Value);

    private void NitIs(string nit) => Assert.AreEqual(nit, _owner.Nit?.ToString());

    private void ContactIs(string email, string phone)
    {
        Assert.AreEqual(email, _owner.Contact?.Email);
        Assert.AreEqual(phone, _owner.Contact?.Phone);
    }

    private void WasRegisteredByTheUser() => Assert.AreEqual(User, _owner.CreatedBy);

    private void NaturalRegistrationIsRecorded()
        => Assert.IsInstanceOfType<NaturalOwnerRegistered>(_owner.DomainEvents.Single());

    private void CompanyRegistrationIsRecorded()
        => Assert.IsInstanceOfType<CompanyOwnerRegistered>(_owner.DomainEvents.Single());
}
