using System.Net;
using NotificationLog.RentalService.Api.Contracts.Owners;
using NotificationLog.RentalService.Application.Owners.Queries.Dtos;
using NotificationLog.RentalService.Domain.Common.Enums;

namespace NotificationLog.RentalService.IntegrationTests.StepDefinitions;

[Binding]
public sealed class OwnerStepDefinitions(RentalsClient client, ScenarioState state)
{
    private readonly RentalsClient _client = client;
    private readonly ScenarioState _state = state;

    [Given("que {string} está registrada/registrado como propietaria/propietario")]
    public Task OwnerIsRegistered(string name) => OwnerIsRegisteredWithDocument(name, _state.NextDocument());

    [Given("que {string} está registrada/registrado como propietaria/propietario con documento {string}")]
    public async Task OwnerIsRegisteredWithDocument(string name, string document)
        => RememberOwner(name, await _client.SetUpAsync<CreatedOwnerResponse>(
            _state.User(name), HttpMethod.Post, "/api/owners/natural", NaturalOwner(name, document)));

    [Given("que {string} está registrada/registrado como propietaria/propietario con NIT {string}")]
    public async Task OwnerIsRegisteredWithNit(string name, string nit)
        => RememberOwner(name, await _client.SetUpAsync<CreatedOwnerResponse>(
            _state.User(name), HttpMethod.Post, "/api/owners/company", CompanyOwner(name, nit)));

    [When("{string} se registra como propietaria/propietario persona natural con:")]
    public async Task RegistersAsNaturalWith(string name, DataTable table)
    {
        var row = table.Rows[0];

        await RegisterAsync(name, "/api/owners/natural", new RegisterNaturalOwnerRequest(
            row["nombres"],
            row["apellidos"],
            Enum.Parse<DocumentType>(row["tipo documento"]),
            row["documento"],
            row["correo"],
            row["teléfono"]));
    }

    [When("{string} se registra como propietaria/propietario persona jurídica con:")]
    public async Task RegistersAsCompanyWith(string name, DataTable table)
    {
        var row = table.Rows[0];

        await RegisterAsync(name, "/api/owners/company", new RegisterCompanyOwnerRequest(
            row["razón social"], row["NIT"], row["correo"], row["teléfono"]));
    }

    [When("{string} se registra como propietaria/propietario persona natural con documento {string}")]
    [When("{string} registra un propietario persona natural con documento {string}")]
    public Task RegistersNaturalWithDocument(string name, string document)
        => RegisterAsync(name, "/api/owners/natural", NaturalOwner(name, document));

    [Given("que {string} registró un propietario persona natural para {string} con documento {string}")]
    [When("{string} registra un propietario persona natural para {string} con documento {string}")]
    public async Task RegistersNaturalOnBehalfOf(string name, string owner, string document)
        => RememberOwner(owner, await _client.SetUpAsync<CreatedOwnerResponse>(
            _state.User(name), HttpMethod.Post, "/api/owners/natural", NaturalOwner(owner, document)));

    [Given("que {string} reclamó ese propietario")]
    public Task HasClaimedThatOwner(string name)
        => _client.SetUpAsync(_state.User(name), HttpMethod.Post, $"/api/owners/{_state.OwnerId}/claim");

    [When("{string} se registra como propietaria/propietario persona jurídica con NIT {string}")]
    public Task RegistersCompanyWithNit(string name, string nit)
        => RegisterAsync(name, "/api/owners/company", CompanyOwner(name, nit));

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

    private async Task RegisterAsync(string name, string url, object body)
    {
        var response = await _client.SendAsync(_state.User(name), HttpMethod.Post, url, body);

        if (response.IsSuccessStatusCode)
            RememberOwner(name, await _client.ReadAsync<CreatedOwnerResponse>());
    }

    private void RememberOwner(string name, CreatedOwnerResponse created)
    {
        _state.OwnerId = created.Id;
        _state.AddOwner(name, created.Id);
    }

    private Task<OwnerDto> RegisteredOwnerAsync() => _client.QueryAsync<OwnerDto>($"/api/owners/{_state.OwnerId}");

    private static RegisterNaturalOwnerRequest NaturalOwner(string name, string document)
        => new(name, "Gómez Rincón", DocumentType.CitizenshipCard, document, $"{name}@example.com", "3001234567");

    private static RegisterCompanyOwnerRequest CompanyOwner(string name, string nit)
        => new($"Inmobiliaria {name}", nit, $"{name}@example.com", "6011234567");

    private static string? ValueOf(OwnerDto owner, string column) => column switch
    {
        "tipo" => owner.Type,
        "nombre" => $"{owner.FirstNames} {owner.LastNames}",
        "documento" => owner.DocumentNumber,
        "razón social" => owner.LegalName,
        "NIT" => owner.Nit,
        "correo" => owner.Email,
        "teléfono" => owner.Phone,
        _ => throw new ArgumentOutOfRangeException(nameof(column), column, null)
    };
}
