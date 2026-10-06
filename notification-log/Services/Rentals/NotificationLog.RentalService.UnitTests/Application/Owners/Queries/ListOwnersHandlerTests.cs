using Application.Shared.Pagination;
using NotificationLog.RentalService.Application.Owners.Queries;
using NotificationLog.RentalService.Application.Owners.Queries.Dtos;
using NotificationLog.RentalService.Application.Owners.Queries.Filters;
using NSubstitute;

namespace NotificationLog.RentalService.UnitTests.Application.Owners.Queries;

[TestClass]
public class ListOwnersHandlerTests : ApplicationScenario
{
    private readonly OwnerFilter _filter = new(Search: "Ana");
    private readonly PageRequest _paging = new(1, 20);
    private PagedResult<OwnerDto> _page = null!;

    [TestMethod]
    public void TheReadModelPageIsReturnedWithItsPagingData() =>
        this.Given(_ => TheReadModelHas(1), "Dado que el modelo de lectura tiene 1 propietario para el filtro")
            .When(_ => IsListed(), "Cuando se lista la página 1 de 20 con ese filtro")
            .Then(_ => ThePageHas(1, 1), "Entonces la página trae 1 propietario de un total de 1")
            .BDDfy("El listado de propietarios devuelve la página del modelo de lectura");

    private void TheReadModelHas(int total)
        => OwnerViews.ListAsync(_filter, _paging, Arg.Any<CancellationToken>())
            .Returns(((IReadOnlyList<OwnerDto>)[ReadModels.Owner(Guid.NewGuid(), Guid.NewGuid())], total));

    private async Task IsListed()
        => _page = await Handler<ListOwnersHandler>().HandleAsync(_filter, _paging, CancellationToken.None);

    private void ThePageHas(int items, int total)
    {
        Assert.HasCount(items, _page.Items);
        Assert.AreEqual(total, _page.TotalCount);
    }
}
