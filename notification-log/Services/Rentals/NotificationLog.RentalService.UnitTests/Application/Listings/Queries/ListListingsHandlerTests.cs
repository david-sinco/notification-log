using Application.Shared.Pagination;
using NotificationLog.RentalService.Application.Listings.Queries;
using NotificationLog.RentalService.Application.Listings.Queries.Dtos;
using NotificationLog.RentalService.Application.Listings.Queries.Filters;
using NotificationLog.RentalService.Domain.Listings.Enums;
using NSubstitute;

namespace NotificationLog.RentalService.UnitTests.Application.Listings.Queries;

[TestClass]
[TestCategory("Application-Listings")]
public class ListListingsHandlerTests : ApplicationScenario
{
    private readonly ListingFilter _filter = new("Chapinero", ListingStatus.Published, Operation.Rent);
    private readonly PageRequest _paging = new(2, 10);
    private PagedResult<ListingSummaryDto> _page = null!;

    [TestMethod]
    public void TheReadModelPageIsReturnedWithItsPagingData() =>
        this.Given(_ => TheReadModelHas(12), "Dado que el modelo de lectura tiene 12 publicaciones para el filtro y devuelve 2 en la página 2")
            .When(_ => IsListed(), "Cuando se lista la página 2 de 10 con ese filtro")
            .Then(_ => ThePageHas(2, 12), "Entonces la página trae 2 publicaciones de un total de 12")
            .And(_ => ThePageIs(2, 10, 2), "Y es la página 2 de 2, con 10 por página")
            .BDDfy("El listado devuelve la página del modelo de lectura con sus datos de paginación");

    private void TheReadModelHas(int total)
        => ListingViews.ListAsync(_filter, _paging, Arg.Any<CancellationToken>())
            .Returns(((IReadOnlyList<ListingSummaryDto>)[ReadModels.ListingSummary(), ReadModels.ListingSummary()], total));

    private async Task IsListed()
        => _page = await Handler<ListListingsHandler>().HandleAsync(_filter, _paging, CancellationToken.None);

    private void ThePageHas(int items, int total)
    {
        Assert.HasCount(items, _page.Items);
        Assert.AreEqual(total, _page.TotalCount);
    }

    private void ThePageIs(int page, int pageSize, int totalPages)
    {
        Assert.AreEqual(page, _page.Page);
        Assert.AreEqual(pageSize, _page.PageSize);
        Assert.AreEqual(totalPages, _page.TotalPages);
    }
}
