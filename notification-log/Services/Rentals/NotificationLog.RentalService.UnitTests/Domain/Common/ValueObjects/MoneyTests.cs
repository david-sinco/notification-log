using NotificationLog.RentalService.Domain.Common.ValueObjects;

namespace NotificationLog.RentalService.UnitTests.Domain.Common.ValueObjects;

[TestClass]
[TestCategory("Domain-Common")]
public class MoneyTests : DomainScenario
{
    private long Amount { get; set; }
    private string Result { get; set; } = string.Empty;

    [TestMethod]
    public void AnAmountInPesosCannotBeNegative() =>
        this.When(_ => MoneyIsCreated(Amount), "Cuando se indica un valor de <amount> pesos")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("amount", "result")
            {
                { -1L, "Un valor en pesos no puede ser negativo." },
                { 0L, Accepted },
                { 1_800_000L, Accepted },
            })
            .BDDfy("Un valor en pesos no puede ser negativo");

    private void MoneyIsCreated(long pesos) => Try(() => Money.Create(pesos));
}
