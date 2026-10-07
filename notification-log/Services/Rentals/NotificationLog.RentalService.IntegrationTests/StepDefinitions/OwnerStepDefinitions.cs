using System.Net;
using NotificationLog.RentalService.Api.Contracts.Owners;
using NotificationLog.RentalService.Application.Owners.Queries.Dtos;

namespace NotificationLog.RentalService.IntegrationTests.StepDefinitions;

[Binding]
public sealed class OwnerStepDefinitions(RentalsClient client, ScenarioState state)
{
    private readonly RentalsClient _client = client;
    private readonly ScenarioState _state = state;

    [Given("que {string} está registrada/registrado como propietaria/propietario")]
    public Task OwnerIsRegistered(string name) => SetUpOwnerAsync(name, name, NewOwner(name));

    [Given("que {string} está registrada/registrado como propietaria/propietario con correo {string}")]
    public Task OwnerIsRegisteredWithEmail(string name, string email)
        => SetUpOwnerAsync(name, name, NewOwner(name) with { Email = email });

    [Given("que {string} está registrada/registrado como propietaria/propietario con teléfono {string}")]
    public Task OwnerIsRegisteredWithPhone(string name, string phone)
        => SetUpOwnerAsync(name, name, NewOwner(name) with { Phone = phone });

    [When("{string} se registra como propietaria/propietario con:")]
    public async Task RegistersWith(string name, DataTable table)
    {
        var row = table.Rows[0];

        await RegisterAsync(name, new RegisterOwnerRequest(row["nombre"], row["correo"], row["teléfono"]));
    }

    [When("{string} se registra como propietaria/propietario")]
    [When("{string} registra un propietario")]
    public Task Registers(string name) => RegisterAsync(name, NewOwner(name));

    [When("{string} se registra como propietaria/propietario con correo {string}")]
    public Task RegistersWithEmail(string name, string email) => RegisterAsync(name, NewOwner(name) with { Email = email });

    [When("{string} se registra como propietaria/propietario con teléfono {string}")]
    public Task RegistersWithPhone(string name, string phone) => RegisterAsync(name, NewOwner(name) with { Phone = phone });

    [Given("que {string} registró un propietario para {string}")]
    public Task RegisteredOnBehalfOf(string name, string owner) => SetUpOwnerAsync(name, owner, NewOwner(owner));

    [Given("que {string} reclamó ese propietario")]
    public Task HasClaimedThatOwner(string name)
        => _client.SetUpAsync(_state.User(name), HttpMethod.Post, $"/api/owners/{_state.OwnerId}/claim");

    [When("{string} consulta el propietario de {string}")]
    public Task QueriesTheOwnerOf(string name, string owner)
        => _client.SendAsync(_state.User(name), HttpMethod.Get, $"/api/owners/{_state.OwnerOf(owner)}");

    [When("{string} consulta los propietarios que puede reclamar")]
    public Task QueriesClaimableOwners(string name)
        => _client.SendAsync(_state.User(name), HttpMethod.Get, "/api/owners/claimable");

    [When("{string} consulta sus propietarios")]
    public Task QueriesTheirOwners(string name) => _client.SendAsync(_state.User(name), HttpMethod.Get, "/api/owners/mine");

    [When("{string} reclama ese propietario")]
    public Task ClaimsThatOwner(string name)
        => _client.SendAsync(_state.User(name), HttpMethod.Post, $"/api/owners/{_state.OwnerId}/claim");

    [When("{string} lista los propietarios")]
    public Task ListsOwners(string name) => _client.SendAsync(_state.User(name), HttpMethod.Get, "/api/owners");

    [Then("el propietario queda registrado y relacionado con {string}")]
    public async Task OwnerIsRegisteredAndRelatedTo(string name)
    {
        Assert.AreEqual(HttpStatusCode.Created, _state.Response!.StatusCode);
        Assert.AreNotEqual(_state.User(name).Id, _state.OwnerId);
        await OwnerIsRelatedTo(name);
    }

    [Then("el propietario queda relacionado con {string}")]
    public async Task OwnerIsRelatedTo(string name)
        => Assert.AreEqual(_state.User(name).Id, (await RegisteredOwnerAsync()).RelatedUserId);

    [Then("el propietario queda sin usuario relacionado")]
    public async Task OwnerHasNoRelatedUser() => Assert.IsNull((await RegisteredOwnerAsync()).RelatedUserId);

    [Then("la respuesta contiene solo ese propietario")]
    public async Task ResponseContainsOnlyThatOwner()
    {
        Assert.AreEqual(HttpStatusCode.OK, _state.Response!.StatusCode);
        Assert.AreEqual(_state.OwnerId, (await _client.ReadAsync<List<OwnerDto>>()).Single().Id);
    }

    [Then("la respuesta no contiene propietarios")]
    public async Task ResponseContainsNoOwners()
    {
        Assert.AreEqual(HttpStatusCode.OK, _state.Response!.StatusCode);
        Assert.IsEmpty(await _client.ReadAsync<List<OwnerDto>>());
    }

    [Then("el propietario queda registrado con un identificador nuevo")]
    public void OwnerIsRegisteredWithANewId()
    {
        Assert.AreEqual(HttpStatusCode.Created, _state.Response!.StatusCode);
        Assert.AreNotEqual(Guid.Empty, _state.OwnerId);
    }

    [Then("{string} puede consultar ese propietario")]
    public async Task CanQueryThatOwner(string name)
    {
        var response = await _client.SendAsync(_state.User(name), HttpMethod.Get, $"/api/owners/{_state.OwnerId}");

        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Then("{string} puede consultar ese propietario con:")]
    public async Task CanQueryThatOwnerWith(string name, DataTable table)
    {
        await CanQueryThatOwner(name);

        var owner = await _client.ReadAsync<OwnerDto>();
        var expected = table.Rows[0];

        foreach (var column in table.Header)
            Assert.AreEqual(expected[column], ValueOf(owner, column), $"Columna '{column}'.");
    }

    private async Task SetUpOwnerAsync(string name, string owner, RegisterOwnerRequest body)
        => RememberOwner(owner, await _client.SetUpAsync<CreatedOwnerResponse>(
            _state.User(name), HttpMethod.Post, "/api/owners", body));

    private async Task RegisterAsync(string name, RegisterOwnerRequest body)
    {
        var response = await _client.SendAsync(_state.User(name), HttpMethod.Post, "/api/owners", body);

        if (response.IsSuccessStatusCode)
            RememberOwner(name, await _client.ReadAsync<CreatedOwnerResponse>());
    }

    private void RememberOwner(string name, CreatedOwnerResponse created)
    {
        _state.OwnerId = created.Id;
        _state.AddOwner(name, created.Id);
    }

    private Task<OwnerDto> RegisteredOwnerAsync() => _client.QueryAsync<OwnerDto>($"/api/owners/{_state.OwnerId}");

    private RegisterOwnerRequest NewOwner(string name)
        => new($"{name} Gómez Rincón", _state.User(name).Email, _state.NextPhone());

    private static string? ValueOf(OwnerDto owner, string column) => column switch
    {
        "nombre" => owner.Name,
        "correo" => owner.Email,
        "teléfono" => owner.Phone,
        _ => throw new ArgumentOutOfRangeException(nameof(column), column, null)
    };
}
