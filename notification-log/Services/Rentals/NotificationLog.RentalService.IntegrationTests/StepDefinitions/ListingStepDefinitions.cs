using System.Globalization;
using System.Net;
using NotificationLog.RentalService.Api.Contracts.Listings;
using NotificationLog.RentalService.Application.Listings.Queries.Dtos;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.IntegrationTests.StepDefinitions;

[Binding]
public sealed class ListingStepDefinitions(RentalsClient client, ScenarioState state, ListingSetup setup)
{
    private readonly RentalsClient _client = client;
    private readonly ScenarioState _state = state;
    private readonly ListingSetup _setup = setup;

    private string ListingUrl => $"/api/listings/{_state.ListingId}";

    [Given("una publicación de arriendo de {string} en estado {string}")]
    public Task AListingInStatus(string owner, string status)
        => _setup.InStatusAsync(_state.User(owner), Operation.Rent, Vocabulary.ListingStatusOf(status));

    [Given("{int} publicaciones de arriendo de {string} en estado {string}")]
    public async Task ManyListingsInStatus(int count, string owner, string status)
    {
        for (var i = 0; i < count; i++)
            await AListingInStatus(owner, status);
    }

    [Given("las siguientes publicaciones de {string}:")]
    public async Task TheFollowingListings(string owner, DataTable table)
    {
        foreach (var row in table.Rows)
            await _setup.InStatusAsync(
                _state.User(owner),
                Vocabulary.OperationOf(row["operación"]),
                Vocabulary.ListingStatusOf(row["estado"]),
                row["barrio"]);
    }

    [Given("una publicación de arriendo de {string} en estado \"Borrador\" con {int} fotos")]
    public Task ADraftWithPhotos(string owner, int photos)
        => _setup.DraftAsync(_state.User(owner), Operation.Rent, photos: photos);

    [Given("una publicación de arriendo de {string} en estado \"Borrador\" con solo {int} fotos")]
    public Task ADraftWithOnlyPhotos(string owner, int photos) => ADraftWithPhotos(owner, photos);

    [Given("una publicación de arriendo de {string} en estado \"Borrador\" sin datos del inmueble")]
    public Task ADraftWithoutDetails(string owner)
        => _setup.DraftAsync(_state.User(owner), Operation.Rent, withDetails: false);

    [Given("una publicación de arriendo de {string} en estado \"Borrador\" sin precio")]
    public Task ADraftWithoutPrice(string owner)
        => _setup.DraftAsync(_state.User(owner), Operation.Rent, withPrice: false);

    [Given("una publicación de arriendo de {string} recién creada")]
    public Task ANewDraft(string owner)
        => _setup.DraftAsync(_state.User(owner), Operation.Rent, withDetails: false, withPrice: false, photos: 0);

    [Given("que {string} fijó el precio en {int}")]
    public Task HasSetThePrice(string name, int price)
        => _client.SetUpAsync(_state.User(name), HttpMethod.Patch, $"{ListingUrl}/price", new ChangeListingPriceRequest(price));

    [Given("que {string} pausó la publicación")]
    public Task HasPausedTheListing(string name)
        => _client.SetUpAsync(_state.User(name), HttpMethod.Post, $"{ListingUrl}/pause");

    [When("{string} crea una publicación de {word} a su nombre")]
    public Task DraftsInOwnName(string name, string operation) => DraftAsync(name, operation, _state.OwnerOf(name));

    [When("{string} crea una publicación de {word} a nombre de {string}")]
    public Task DraftsOnBehalfOf(string name, string operation, string owner)
        => DraftAsync(name, operation, _state.OwnerOf(owner));

    [When("{string} crea una publicación de {word} a nombre de un propietario inexistente")]
    public Task DraftsForAMissingOwner(string name, string operation) => DraftAsync(name, operation, Guid.NewGuid());

    [When("{string} actualiza los datos del inmueble con:")]
    public Task UpdatesDetailsWith(string name, DataTable table)
    {
        var row = table.Rows[0];

        return UpdateDetailsAsync(name, new UpdateListingDetailsRequest(
            Enum.Parse<PropertyType>(row["tipo"]),
            decimal.Parse(row["área"], CultureInfo.InvariantCulture),
            int.Parse(row["habitaciones"]),
            int.Parse(row["baños"]),
            int.Parse(row["parqueaderos"]),
            int.Parse(row["estrato"]),
            int.Parse(row["piso"]),
            row["ascensor"] == "sí",
            long.Parse(row["administración"]),
            row["ciudad"],
            row["barrio"],
            row["dirección"],
            new string('a', 120)));
    }

    [When("{string} actualiza los datos del inmueble con una descripción de {int} caracteres")]
    public Task UpdatesDetailsWithDescriptionOf(string name, int length)
        => UpdateDetailsAsync(name, ListingSetup.Details(descriptionLength: length));

    [When("{string} actualiza los datos del inmueble cambiando el barrio a {string}")]
    public Task UpdatesTheNeighborhood(string name, string neighborhood)
        => UpdateDetailsAsync(name, ListingSetup.Details(neighborhood));

    [When("{string} fija el precio en {int}")]
    public Task SetsThePrice(string name, int price)
        => _client.SendAsync(_state.User(name), HttpMethod.Patch, $"{ListingUrl}/price", new ChangeListingPriceRequest(price));

    [When("{string} sube {int} foto/fotos")]
    public async Task UploadsPhotos(string name, int count)
    {
        for (var i = 0; i < count; i++)
            await _client.UploadPhotoAsync(_state.User(name), _state.ListingId);
    }

    [When("{string} envía la publicación a revisión")]
    public Task Submits(string name) => PostAsync(name, "submit");

    [When("{string} pausa la publicación")]
    public Task Pauses(string name) => PostAsync(name, "pause");

    [When("{string} reanuda la publicación")]
    public Task Resumes(string name) => PostAsync(name, "resume");

    [When("{string} renueva la publicación")]
    public Task Renews(string name) => PostAsync(name, "renew");

    [When("{string} cierra la publicación con valor final {int} firmada el {string}")]
    public Task Closes(string name, int finalPrice, string signedOn)
        => _client.SendAsync(
            _state.User(name),
            HttpMethod.Post,
            $"{ListingUrl}/close",
            new CloseListingRequest(finalPrice, DateOnly.ParseExact(signedOn, "yyyy-MM-dd", CultureInfo.InvariantCulture)));

    [When("{string} retira la publicación con el motivo {string}")]
    public Task Withdraws(string name, string reason)
        => _client.SendAsync(_state.User(name), HttpMethod.Post, $"{ListingUrl}/withdraw", new WithdrawListingRequest(reason));

    [When("{string} reordena las fotos poniendo la última en primer lugar")]
    public async Task MovesTheLastPhotoFirst(string name)
    {
        var photos = await PhotosAsync();
        _state.PhotosBeforeLastChange = photos;

        await _client.SendAsync(
            _state.User(name),
            HttpMethod.Put,
            $"{ListingUrl}/photos/order",
            new ReorderListingPhotosRequest([photos[^1], .. photos.Take(photos.Count - 1)]));
    }

    [When("{string} quita la primera foto")]
    public async Task RemovesTheFirstPhoto(string name)
    {
        var photos = await PhotosAsync();
        _state.PhotosBeforeLastChange = photos;

        await _client.SendAsync(_state.User(name), HttpMethod.Delete, $"{ListingUrl}/photos/{photos[0]}");
    }

    [When("{string} consulta una publicación inexistente")]
    public Task QueriesAMissingListing(string name)
        => _client.SendAsync(_state.User(name), HttpMethod.Get, $"/api/listings/{Guid.NewGuid()}");

    [Then("la publicación queda en estado {string}")]
    public async Task ListingStatusIs(string status)
        => Assert.AreEqual(Vocabulary.ListingStatusOf(status).ToString(), (await ListingAsync()).Status);

    [Then("la publicación pertenece a {string}")]
    public async Task ListingBelongsTo(string owner)
        => Assert.AreEqual(_state.OwnerOf(owner), (await ListingAsync()).OwnerId);

    [Then("la publicación tiene {int} fotos")]
    public async Task ListingHasPhotos(int count) => Assert.HasCount(count, await PhotosAsync());

    [Then("la publicación tiene precio {int}")]
    public async Task ListingPriceIs(int price)
    {
        Assert.IsTrue(_state.Response!.IsSuccessStatusCode, await _state.Response.Content.ReadAsStringAsync());
        Assert.AreEqual(price, (await ListingAsync()).Price);
    }

    [Then("la publicación vence el {string}")]
    public async Task ListingExpiresOn(string date)
    {
        Assert.IsTrue(_state.Response!.IsSuccessStatusCode, await _state.Response.Content.ReadAsStringAsync());

        var expiresAt = (await ListingAsync()).ExpiresAt;

        Assert.IsNotNull(expiresAt);
        Assert.AreEqual(DateOnly.ParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture), ColombiaTime.DateOf(expiresAt.Value));
    }

    [Then("la primera foto de la publicación es la que antes era la última")]
    public async Task TheFirstPhotoWasTheLast()
        => Assert.AreEqual(_state.PhotosBeforeLastChange[^1], (await PhotosAsync())[0]);

    [Then("la publicación no se encuentra")]
    public void ListingIsNotFound() => Assert.AreEqual(HttpStatusCode.NotFound, _state.Response!.StatusCode);

    [Then("la publicación aparece en el catálogo")]
    public async Task IsInTheCatalog() => Assert.AreEqual(HttpStatusCode.OK, await CatalogStatusAsync());

    [Then("la publicación no aparece en el catálogo")]
    public async Task IsNotInTheCatalog() => Assert.AreEqual(HttpStatusCode.NotFound, await CatalogStatusAsync());

    private async Task DraftAsync(string name, string operation, Guid ownerId)
    {
        var response = await _client.SendAsync(
            _state.User(name), HttpMethod.Post, "/api/listings", new DraftListingRequest(ownerId, Vocabulary.OperationOf(operation)));

        if (response.IsSuccessStatusCode)
            _state.ListingId = (await _client.ReadAsync<CreatedListingResponse>()).Id;
    }

    private Task UpdateDetailsAsync(string name, UpdateListingDetailsRequest details)
        => _client.SendAsync(_state.User(name), HttpMethod.Put, $"{ListingUrl}/details", details);

    private Task PostAsync(string name, string action)
        => _client.SendAsync(_state.User(name), HttpMethod.Post, $"{ListingUrl}/{action}");

    private Task<ListingDto> ListingAsync() => _client.QueryAsync<ListingDto>(ListingUrl);

    private async Task<IReadOnlyList<string>> PhotosAsync()
        => (await ListingAsync()).Photos.Select(photo => photo.FileName).ToList();

    private async Task<HttpStatusCode> CatalogStatusAsync()
    {
        using var response = await _client.ProbeAsync(null, $"/api/catalog/{_state.ListingId}");

        return response.StatusCode;
    }
}
