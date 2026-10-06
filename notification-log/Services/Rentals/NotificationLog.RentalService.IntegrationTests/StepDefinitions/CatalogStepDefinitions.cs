using System.Net;
using NotificationLog.RentalService.Application.Listings.Queries.Dtos;

namespace NotificationLog.RentalService.IntegrationTests.StepDefinitions;

[Binding]
public sealed class CatalogStepDefinitions(RentalsClient client, ScenarioState state)
{
    private const string CatalogUrl = "/api/catalog";

    private readonly RentalsClient _client = client;
    private readonly ScenarioState _state = state;

    [When("un visitante anónimo consulta el catálogo")]
    public Task BrowsesTheCatalog() => _client.SendAsync(null, HttpMethod.Get, CatalogUrl);

    [When("un visitante anónimo consulta el catálogo filtrando por operación {string}")]
    public Task BrowsesByOperation(string operation)
        => _client.SendAsync(null, HttpMethod.Get, $"{CatalogUrl}?operation={operation}");

    [When("un visitante anónimo busca {string} en el catálogo")]
    public Task SearchesTheCatalog(string search)
        => _client.SendAsync(null, HttpMethod.Get, $"{CatalogUrl}?search={Uri.EscapeDataString(search)}");

    [When("un visitante anónimo consulta la página {int} del catálogo con {int} publicaciones por página")]
    public Task BrowsesAPage(int page, int pageSize)
        => _client.SendAsync(null, HttpMethod.Get, $"{CatalogUrl}?page={page}&pageSize={pageSize}");

    [When("un visitante anónimo consulta la publicación en el catálogo")]
    public Task OpensTheListing() => _client.SendAsync(null, HttpMethod.Get, $"{CatalogUrl}/{_state.ListingId}");

    [When("un visitante anónimo lista las publicaciones por la API privada")]
    public Task ListsThroughThePrivateApi() => _client.SendAsync(null, HttpMethod.Get, "/api/listings");

    [Then("el catálogo muestra {int} publicación/publicaciones")]
    public async Task CatalogShows(int count) => Assert.HasCount(count, (await PageAsync()).Items);

    [Then("el catálogo muestra las publicaciones de los barrios:")]
    public async Task CatalogShowsNeighborhoods(DataTable table)
        => CollectionAssert.AreEquivalent(
            table.Rows.Select(row => row["barrio"]).ToList(),
            (await PageAsync()).Items.Select(listing => listing.Neighborhood).ToList());

    [Then("el total de publicaciones del catálogo es {int}")]
    public async Task CatalogTotalIs(int total) => Assert.AreEqual(total, (await PageAsync()).TotalCount);

    [Then("ve la publicación con sus {int} fotos y su precio")]
    public async Task SeesTheListing(int photos)
    {
        Assert.AreEqual(HttpStatusCode.OK, _state.Response!.StatusCode);

        var listing = await _client.ReadAsync<ListingDto>();

        Assert.HasCount(photos, listing.Photos);
        Assert.IsTrue(listing.Photos.All(photo => !string.IsNullOrWhiteSpace(photo.Url)));
        Assert.IsNotNull(listing.Price);
    }

    private async Task<PageOf<ListingSummaryDto>> PageAsync()
    {
        Assert.AreEqual(HttpStatusCode.OK, _state.Response!.StatusCode);

        return await _client.ReadAsync<PageOf<ListingSummaryDto>>();
    }
}
