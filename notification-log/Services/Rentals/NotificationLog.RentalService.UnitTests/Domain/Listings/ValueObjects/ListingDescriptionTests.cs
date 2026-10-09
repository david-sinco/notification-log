using NotificationLog.RentalService.Domain.Listings.ValueObjects;

namespace NotificationLog.RentalService.UnitTests.Domain.Listings.ValueObjects;

[TestClass]
[TestCategory("Domain-Listings")]
public class ListingDescriptionTests : DomainScenario
{
    private int Length { get; set; }
    private string Result { get; set; } = string.Empty;

    [TestMethod]
    public void TheDescriptionHasBetween100And2000Characters() =>
        this.When(_ => DescriptionOfLengthIsWritten(Length), "Cuando se escribe una descripción de <length> caracteres")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("length", "result")
            {
                { 0, "La descripción es obligatoria." },
                { 99, "La descripción debe tener al menos 100 caracteres." },
                { 100, Accepted },
                { 2000, Accepted },
                { 2001, "La descripción no puede superar 2000 caracteres." },
            })
            .BDDfy("La descripción tiene entre 100 y 2000 caracteres");

    [TestMethod]
    public void SurroundingSpacesDoNotCount() =>
        this.When(_ => DescriptionOf99CharactersSurroundedBySpacesIsWritten(),
                "Cuando se escribe una descripción de 99 caracteres rodeada de espacios")
            .Then(_ => IsRejectedWith("La descripción debe tener al menos 100 caracteres."),
                "Entonces se rechaza: La descripción debe tener al menos 100 caracteres.")
            .BDDfy("Los espacios de los extremos no cuentan");

    private void DescriptionOfLengthIsWritten(int length) => Try(() => ListingDescription.Create(new string('a', length)));

    private void DescriptionOf99CharactersSurroundedBySpacesIsWritten()
        => Try(() => ListingDescription.Create($"   {new string('a', 99)}   "));
}
