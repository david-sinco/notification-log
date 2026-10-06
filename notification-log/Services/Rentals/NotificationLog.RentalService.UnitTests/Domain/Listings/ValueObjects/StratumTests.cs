using NotificationLog.RentalService.Domain.Listings.ValueObjects;

namespace NotificationLog.RentalService.UnitTests.Domain.Listings.ValueObjects;

[TestClass]
public class StratumTests : DomainScenario
{
    private int Value { get; set; }
    private string Result { get; set; } = string.Empty;

    [TestMethod]
    public void TheStratumGoesFrom1To6() =>
        this.When(_ => StratumIsCreated(Value), "Cuando se indica el estrato <value>")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("value", "result")
            {
                { 0, "El estrato debe estar entre 1 y 6." },
                { 1, Accepted },
                { 6, Accepted },
                { 7, "El estrato debe estar entre 1 y 6." },
            })
            .BDDfy("El estrato va de 1 a 6");

    private void StratumIsCreated(int stratum) => Try(() => Stratum.Create(stratum));
}
