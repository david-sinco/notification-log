using NotificationLog.RentalService.Application.Visitors.Queries;
using NotificationLog.RentalService.Application.Visitors.Queries.Dtos;
using NSubstitute;

namespace NotificationLog.RentalService.UnitTests.Application.Visitors.Queries;

[TestClass]
public class GetVisitorsSummaryHandlerTests : ApplicationScenario
{
    private VisitorsSummaryDto _summary = null!;

    [TestMethod]
    public void NewVisitorsAreCountedFromTheLastSevenDays() =>
        this.Given(_ => ItIs("2026-10-08 15:00"), "Dado que son las 2026-10-08 15:00")
            .And(_ => TheReadModelSummarizes(), "Y el modelo de lectura resume 46 visitantes, 9 nuevos y 17 sin visitas")
            .When(_ => IsSummarized(), "Cuando se pide el resumen de visitantes")
            .Then(_ => NewVisitorsAreCountedSince("2026-10-01 15:00"), "Entonces los nuevos se cuentan desde el 2026-10-01 15:00")
            .And(_ => TheSummaryIs(46, 9, 17), "Y el resumen trae 46 visitantes, 9 nuevos y 17 sin visitas")
            .BDDfy("Los visitantes nuevos son los de los últimos 7 días");

    private void TheReadModelSummarizes()
        => VisitorViews.GetSummaryAsync(Arg.Any<DateTimeOffset>(), Arg.Any<CancellationToken>())
            .Returns(new VisitorsSummaryDto(46, 9, 17));

    private async Task IsSummarized() => _summary = await Handler<GetVisitorsSummaryHandler>().HandleAsync(CancellationToken.None);

    private void NewVisitorsAreCountedSince(string moment)
        => VisitorViews.Received(1).GetSummaryAsync(Clock.At(moment), Arg.Any<CancellationToken>());

    private void TheSummaryIs(int total, int newLastWeek, int withoutVisits)
        => Assert.AreEqual(new VisitorsSummaryDto(total, newLastWeek, withoutVisits), _summary);
}
