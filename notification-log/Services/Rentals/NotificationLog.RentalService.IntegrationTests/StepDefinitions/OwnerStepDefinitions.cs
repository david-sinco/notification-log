using System.Net;
using NotificationLog.RentalService.Api.Contracts.Owners;
using NotificationLog.RentalService.Application.Owners.Queries.Dtos;
using NotificationLog.RentalService.Domain.Common.Enums;

namespace NotificationLog.RentalService.IntegrationTests.StepDefinitions;

[Binding]
public sealed class OwnerStepDefinitions(RentalsApi api, RentalsClient client, ScenarioState state)
{
    private readonly RentalsApi _api = api;
    private readonly RentalsClient _client = client;
    private readonly ScenarioState _state = state;

    [Given("que {string} está registrada/registrado como propietaria/propietario")]
    public Task OwnerIsRegistered(string name) => OwnerIsRegisteredWithDocument(name, _state.NextDocument());

    [Given("que {string} está registrada/registrado como propietaria/propietario con documento {string}")]
    public async Task OwnerIsRegisteredWithDocument(string name, string document)
        => await RememberOwnerAsync(await _client.SetUpAsync<CreatedOwnerResponse>(
            _state.User(name), HttpMethod.Post, "/api/owners/natural", NaturalOwner(name, document)));

    [Given("que {string} está registrada/registrado como propietaria/propietario con NIT {string}")]
    public async Task OwnerIsRegisteredWithNit(string name, string nit)
        => await RememberOwnerAsync(await _client.SetUpAsync<CreatedOwnerResponse>(
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

    [When("{string} se registra como propietaria/propietario persona jurídica con NIT {string}")]
    public Task RegistersCompanyWithNit(string name, string nit)
        => RegisterAsync(name, "/api/owners/company", CompanyOwner(name, nit));

    [When("{string} consulta el propietario de {string}")]
    public Task QueriesTheOwnerOf(string name, string owner)
        => _client.SendAsync(_state.User(name), HttpMethod.Get, $"/api/owners/{_state.User(owner).Id}");

    [When("{string} lista los propietarios")]
    public Task ListsOwners(string name) => _client.SendAsync(_state.User(name), HttpMethod.Get, "/api/owners");

    [Then("el propietario queda registrado con el identificador de {string}")]
    public void OwnerIsRegisteredWithTheIdOf(string name)
    {
        Assert.AreEqual(HttpStatusCode.Created, _state.Response!.StatusCode);
        Assert.AreEqual(_state.User(name).Id, _state.OwnerId);
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

    [Then("se solicita una cuenta con rol {word} para {string}")]
    public void AnAccountIsRequested(string role, string email)
    {
        var request = _api.Accounts.Requested.Single();

        Assert.AreEqual(role, request.Role);
        Assert.AreEqual(email, request.Email);
        Assert.AreEqual(_state.OwnerId.ToString(), request.UserId);
    }

    private async Task RegisterAsync(string name, string url, object body)
    {
        var response = await _client.SendAsync(_state.User(name), HttpMethod.Post, url, body);

        if (response.IsSuccessStatusCode)
            await RememberOwnerAsync(await _client.ReadAsync<CreatedOwnerResponse>());
    }

    private Task RememberOwnerAsync(CreatedOwnerResponse created)
    {
        _state.OwnerId = created.Id;

        return Task.CompletedTask;
    }

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
