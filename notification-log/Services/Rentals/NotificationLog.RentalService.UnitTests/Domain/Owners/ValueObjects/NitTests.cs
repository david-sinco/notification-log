using NotificationLog.RentalService.Domain.Owners.ValueObjects;

namespace NotificationLog.RentalService.UnitTests.Domain.Owners.ValueObjects;

[TestClass]
public class NitTests : DomainScenario
{
    private Nit _nit = null!;

    private string Value { get; set; } = string.Empty;
    private string Result { get; set; } = string.Empty;

    [TestMethod]
    public void TheNitHasEightOrNineDigitsAndAValidCheckDigit() =>
        this.When(_ => NitIsCreated(Value), "Cuando se indica el NIT «<value>»")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("value", "result")
            {
                { "900123456-8", Accepted },
                { "80012345-9", Accepted },
                { "900.123.456-8", Accepted },
                { "900123456-1", "El dígito de verificación del NIT no es correcto." },
                { "9001234568", "El NIT debe tener el formato 900123456-7." },
                { "1234567-8", "El NIT debe tener el formato 900123456-7." },
                { "", "El NIT es obligatorio." },
            })
            .BDDfy("El NIT tiene ocho o nueve dígitos y un dígito de verificación correcto");

    [TestMethod]
    public void TheNitIsStoredWithoutDotsAndWithItsCheckDigit() =>
        this.When(_ => NitIsCreated("900.123.456-8"), "Cuando se indica el NIT 900.123.456-8")
            .Then(_ => NumberIs("900123456"), "Entonces el número es 900123456")
            .And(_ => CheckDigitIs(8), "Y el dígito de verificación es 8")
            .BDDfy("El NIT se guarda sin puntos y con su dígito de verificación");

    private void NitIsCreated(string nit) => Try(() => _nit = Nit.Create(nit));

    private void NumberIs(string number) => Assert.AreEqual(number, _nit.Number);

    private void CheckDigitIs(int digit) => Assert.AreEqual(digit, _nit.CheckDigit);
}
