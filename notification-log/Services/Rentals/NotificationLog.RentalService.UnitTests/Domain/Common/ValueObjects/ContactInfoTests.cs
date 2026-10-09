using NotificationLog.RentalService.Domain.Common.ValueObjects;

namespace NotificationLog.RentalService.UnitTests.Domain.Common.ValueObjects;

[TestClass]
[TestCategory("Domain-Common")]
public class ContactInfoTests : DomainScenario
{
    private ContactInfo _contact = null!;

    private string Phone { get; set; } = string.Empty;
    private string Email { get; set; } = string.Empty;
    private string Stored { get; set; } = string.Empty;
    private string Result { get; set; } = string.Empty;

    [TestMethod]
    public void ThePhoneIsStoredInInternationalFormatDefaultingToColombia() =>
        this.When(_ => ContactIsCreatedWithPhone(Phone), "Cuando se indica el teléfono «<phone>»")
            .Then(_ => PhoneIsStoredAs(Stored), "Entonces el teléfono se guarda como <stored>")
            .WithExamples(new ExampleTable("phone", "stored")
            {
                { "3001234567", "+573001234567" },
                { "300 123 4567", "+573001234567" },
                { "(601) 123-4567", "+576011234567" },
                { "+57 300 123 4567", "+573001234567" },
                { "0057 3001234567", "+573001234567" },
                { "+1 415 555 0100", "+14155550100" },
            })
            .BDDfy("El teléfono se guarda en formato internacional, con Colombia por defecto");

    [TestMethod]
    public void APhoneWithoutAValidFormatIsRejected() =>
        this.When(_ => ContactIsCreatedWithPhone(Phone), "Cuando se indica el teléfono «<phone>»")
            .Then(_ => IsRejectedWith(Result), "Entonces se rechaza: <result>")
            .WithExamples(new ExampleTable("phone", "result")
            {
                { "", "El teléfono del propietario es obligatorio." },
                { "123", "El teléfono no tiene un formato válido." },
                { "300ABC4567", "El teléfono no tiene un formato válido." },
                { "+0 300 123 4567", "El teléfono no tiene un formato válido." },
            })
            .BDDfy("Un teléfono sin formato válido se rechaza");

    [TestMethod]
    public void TheEmailMustHaveAValidFormat() =>
        this.When(_ => ContactIsCreatedWithEmail(Email), "Cuando se indica el correo «<email>»")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("email", "result")
            {
                { "ana@example.com", Accepted },
                { "ana.gomez@correo.example.co", Accepted },
                { "", "El correo del propietario es obligatorio." },
                { "ana@example", "El correo no tiene un formato válido." },
                { "ana example@x.com", "El correo no tiene un formato válido." },
                { "ana.example.com", "El correo no tiene un formato válido." },
            })
            .BDDfy("El correo debe tener un formato válido");

    [TestMethod]
    public void TheEmailIsStoredInLowercaseAndTrimmed() =>
        this.When(_ => ContactIsCreatedWithEmail("  Ana@Example.COM  "), "Cuando se indica el correo «  Ana@Example.COM  »")
            .Then(_ => EmailIsStoredAs("ana@example.com"), "Entonces el correo se guarda como ana@example.com")
            .BDDfy("El correo se guarda en minúsculas y sin espacios");

    private void ContactIsCreatedWithPhone(string phone) => Try(() => _contact = ContactInfo.Create("ana@example.com", phone));

    private void ContactIsCreatedWithEmail(string email) => Try(() => _contact = ContactInfo.Create(email, "3001234567"));

    private void PhoneIsStoredAs(string expected) => Assert.AreEqual(expected, _contact.Phone);

    private void EmailIsStoredAs(string expected) => Assert.AreEqual(expected, _contact.Email);
}
