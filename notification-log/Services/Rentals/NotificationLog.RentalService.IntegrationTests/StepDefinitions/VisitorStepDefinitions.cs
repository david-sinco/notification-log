using System.Net;
using NotificationLog.RentalService.Api.Contracts.Visitors;
using NotificationLog.RentalService.Application.Visitors.Queries.Dtos;
using NotificationLog.RentalService.Domain.Common.Enums;

namespace NotificationLog.RentalService.IntegrationTests.StepDefinitions;

[Binding]
public sealed class VisitorStepDefinitions(RentalsClient client, ScenarioState state)
{
    private const string ProfileUrl = "/api/visitors/me/profile";

    private readonly RentalsClient _client = client;
    private readonly ScenarioState _state = state;

    [Given("que {string} completó su perfil de visitante")]
    public Task HasCompletedTheProfile(string name) => HasCompletedTheProfileWithDocument(name, _state.NextDocument());

    [Given("que {string} completó su perfil de visitante con documento {string}")]
    public Task HasCompletedTheProfileWithDocument(string name, string document)
        => _client.SetUpAsync(_state.User(name), HttpMethod.Put, ProfileUrl, Profile(name, document));

    [When("{string} completa su perfil de visitante con documento {string}")]
    public Task CompletesTheProfileWithDocument(string name, string document)
        => _client.SendAsync(_state.User(name), HttpMethod.Put, ProfileUrl, Profile(name, document));

    [When("{string} completa su perfil de visitante con:")]
    public Task CompletesTheProfileWith(string name, DataTable table)
    {
        var row = table.Rows[0];

        return _client.SendAsync(_state.User(name), HttpMethod.Put, ProfileUrl, new CompleteVisitorProfileRequest(
            row["nombres"],
            row["apellidos"],
            Enum.Parse<DocumentType>(row["tipo documento"]),
            row["documento"],
            row["correo"],
            row["teléfono"]));
    }

    [When("{string} consulta su registro de visitante")]
    public Task QueriesOwnRecord(string name) => _client.SendAsync(_state.User(name), HttpMethod.Get, "/api/visitors/me");

    [When("{string} consulta el registro de visitante de {string}")]
    public Task QueriesTheRecordOf(string name, string visitor)
        => _client.SendAsync(_state.User(name), HttpMethod.Get, $"/api/visitors/{_state.User(visitor).Id}");

    [When("{string} lista los visitantes")]
    public Task ListsVisitors(string name) => _client.SendAsync(_state.User(name), HttpMethod.Get, "/api/visitors");

    [Then("el registro de visitante no se encuentra")]
    public void RecordIsNotFound() => Assert.AreEqual(HttpStatusCode.NotFound, _state.Response!.StatusCode);

    [Then("{string} consulta su registro de visitante y ve:")]
    public async Task QueriesOwnRecordAndSees(string name, DataTable table)
    {
        Assert.IsTrue(_state.Response!.IsSuccessStatusCode, await _state.Response.Content.ReadAsStringAsync());

        await QueriesOwnRecord(name);

        var visitor = await _client.ReadAsync<VisitorDto>();
        var expected = table.Rows[0];

        foreach (var column in table.Header)
            Assert.AreEqual(expected[column], ValueOf(visitor, column), $"Columna '{column}'.");
    }

    [Then("el visitante está en estado {word}")]
    public async Task VisitorStatusIs(string status)
    {
        Assert.AreEqual(HttpStatusCode.OK, _state.Response!.StatusCode);
        Assert.AreEqual(status, (await _client.ReadAsync<VisitorDto>()).Status);
    }

    private static CompleteVisitorProfileRequest Profile(string name, string document)
        => new(name, "Rojas Peña", DocumentType.CitizenshipCard, document, $"{name}@example.com", "3109876543");

    private static string? ValueOf(VisitorDto visitor, string column) => column switch
    {
        "estado" => visitor.Status,
        "nombre" => visitor.DisplayName,
        "documento" => visitor.DocumentNumber,
        "correo" => visitor.Email,
        "teléfono" => visitor.Phone,
        _ => throw new ArgumentOutOfRangeException(nameof(column), column, null)
    };
}
