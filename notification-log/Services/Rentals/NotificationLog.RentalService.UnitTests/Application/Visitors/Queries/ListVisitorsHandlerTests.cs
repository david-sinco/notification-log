using Application.Shared.Pagination;
using NotificationLog.RentalService.Application.Visitors.Queries;
using NotificationLog.RentalService.Application.Visitors.Queries.Dtos;
using NotificationLog.RentalService.Application.Visitors.Queries.Filters;
using NSubstitute;

namespace NotificationLog.RentalService.UnitTests.Application.Visitors.Queries;

[TestClass]
public class ListVisitorsHandlerTests : ApplicationScenario
{
    private readonly PageRequest _paging = new(1, 20);
    private readonly VisitorFilter _filter = new("rojas", HasVisits: true, VisitorSort.MostVisits);
    private PagedResult<VisitorDto> _page = null!;

    [TestMethod]
    public void TheReadModelPageIsReturnedWithItsPagingData() =>
        this.Given(_ => TheReadModelHas(1), "Dado que el modelo de lectura tiene 1 visitante")
            .When(_ => IsListed(), "Cuando se lista la página 1 de 20 buscando «rojas», con visitas y por más visitas")
            .Then(_ => ThePageHas(1, 1), "Entonces la página trae 1 visitante de un total de 1")
            .And(_ => TheFilterReachesTheReadModel(), "Y el filtro llega tal cual al modelo de lectura")
            .BDDfy("El listado de visitantes devuelve la página filtrada del modelo de lectura");

    private void TheReadModelHas(int total)
        => VisitorViews.ListAsync(_filter, _paging, Arg.Any<CancellationToken>())
            .Returns(((IReadOnlyList<VisitorDto>)[ReadModels.Visitor(Guid.NewGuid())], total));

    private async Task IsListed()
        => _page = await Handler<ListVisitorsHandler>().HandleAsync(_filter, _paging, CancellationToken.None);

    private void ThePageHas(int items, int total)
    {
        Assert.HasCount(items, _page.Items);
        Assert.AreEqual(total, _page.TotalCount);
    }

    private void TheFilterReachesTheReadModel()
        => VisitorViews.Received(1).ListAsync(_filter, _paging, Arg.Any<CancellationToken>());
}
