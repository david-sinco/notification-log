using NotificationLog.RentalService.Domain.Common.Enums;
using NotificationLog.RentalService.Domain.Common.ValueObjects;

namespace NotificationLog.RentalService.UnitTests.Domain.Common.ValueObjects;

[TestClass]
public class IdentityDocumentTests : DomainScenario
{
    private const string InvalidFormat = "El número de documento no tiene un formato válido.";

    private IdentityDocument _document = null!;

    private DocumentType Type { get; set; }
    private string Number { get; set; } = string.Empty;
    private string Stored { get; set; } = string.Empty;
    private string Result { get; set; } = string.Empty;

    [TestMethod]
    public void TheNumberFormatDependsOnTheDocumentType() =>
        this.When(_ => DocumentIsCreated(Type, Number), "Cuando se indica un documento <type> con número «<number>»")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("type", "number", "result")
            {
                { DocumentType.CitizenshipCard, "123456", Accepted },
                { DocumentType.CitizenshipCard, "1234567890", Accepted },
                { DocumentType.CitizenshipCard, "12345", InvalidFormat },
                { DocumentType.CitizenshipCard, "12345678901", InvalidFormat },
                { DocumentType.CitizenshipCard, "12A456", InvalidFormat },
                { DocumentType.CitizenshipCard, "", "El número de documento es obligatorio." },
                { DocumentType.ForeignerId, "654321", Accepted },
                { DocumentType.ForeignerId, "AB1234", InvalidFormat },
                { DocumentType.Passport, "AB123", Accepted },
                { DocumentType.Passport, "AB12", InvalidFormat },
                { DocumentType.Passport, "AB-12345", InvalidFormat },
            })
            .BDDfy("El formato del número depende del tipo de documento");

    [TestMethod]
    public void TheNumberIsStoredWithoutDotsOrSpacesAndInUppercase() =>
        this.When(_ => DocumentIsCreated(Type, Number), "Cuando se indica un documento <type> con número «<number>»")
            .Then(_ => NumberIsStoredAs(Stored), "Entonces el número se guarda como <stored>")
            .WithExamples(new ExampleTable("type", "number", "stored")
            {
                { DocumentType.CitizenshipCard, "52.123.456", "52123456" },
                { DocumentType.CitizenshipCard, "52 123 456", "52123456" },
                { DocumentType.Passport, "ab 12345", "AB12345" },
            })
            .BDDfy("El número se guarda sin puntos ni espacios y en mayúsculas");

    private void DocumentIsCreated(DocumentType type, string number)
        => Try(() => _document = IdentityDocument.Create(type, number));

    private void NumberIsStoredAs(string expected) => Assert.AreEqual(expected, _document.Number);
}
