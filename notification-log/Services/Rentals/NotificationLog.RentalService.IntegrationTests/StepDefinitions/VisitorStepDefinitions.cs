using System.Net;
using Google.Protobuf.WellKnownTypes;
using NotificationLog.Contracts.Identity;
using NotificationLog.RentalService.Application.Visitors.Queries.Dtos;

namespace NotificationLog.RentalService.IntegrationTests.StepDefinitions;

[Binding]
public sealed class VisitorStepDefinitions(RentalsApi api, RentalsClient client, ScenarioState state)
{
    private readonly RentalsApi _api = api;
    private readonly RentalsClient _client = client;
    private readonly ScenarioState _state = state;

    [When("{string} confirma el correo {string}")]
    public Task ConfirmsEmail(string name, string email)
        => _api.PublishAsync(new PersonVerificationChanged
        {
            EventId = Guid.NewGuid().ToString(),
            OccurredAt = Timestamp.FromDateTimeOffset(_api.Clock.GetUtcNow()),
            SchemaVersion = 1,
            UserId = _state.User(name).Id.ToString(),
            Email = email,
            Phone = string.Empty
        });

    [When("{string} consulta su registro de visitante")]
    public Task QueriesOwnRecord(string name) => QueriesTheRecordOf(name, name);

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
        await QueriesOwnRecord(name);

        Assert.IsTrue(_state.Response!.IsSuccessStatusCode, await _state.Response.Content.ReadAsStringAsync());

        var visitor = await _client.ReadAsync<VisitorDto>();
        var expected = table.Rows[0];

        foreach (var column in table.Header)
            Assert.AreEqual(expected[column], ValueOf(visitor, column), $"Columna '{column}'.");
    }

    private static string ValueOf(VisitorDto visitor, string column) => column switch
    {
        "nombre" => visitor.Name,
        "correo" => visitor.Email,
        "teléfono" => visitor.Phone,
        _ => throw new ArgumentOutOfRangeException(nameof(column), column, null)
    };
}
