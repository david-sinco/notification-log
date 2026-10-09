using System.Net;
using Google.Protobuf.WellKnownTypes;
using NotificationLog.Contracts.Identity;
using NotificationLog.RentalService.Application.Visitors.Queries.Dtos;

namespace NotificationLog.RentalService.IntegrationTests.StepDefinitions;

[Binding]
public sealed class VisitorStepDefinitions(RentalsApi api, RentalsClient client, ScenarioState state)
{
    private const string VisitorsUrl = "/api/visitors";

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
        => _client.SendAsync(_state.User(name), HttpMethod.Get, $"{VisitorsUrl}/{_state.User(visitor).Id}");

    [When("{string} lista los visitantes")]
    public Task ListsVisitors(string name) => _client.SendAsync(_state.User(name), HttpMethod.Get, VisitorsUrl);

    [When("{string} busca visitantes con {string}")]
    public Task SearchesVisitors(string name, string search)
        => _client.SendAsync(_state.User(name), HttpMethod.Get, $"{VisitorsUrl}?search={Uri.EscapeDataString(search)}");

    [When("{string} lista los visitantes con visitas")]
    public Task ListsVisitorsWithVisits(string name) => _client.SendAsync(_state.User(name), HttpMethod.Get, $"{VisitorsUrl}?hasVisits=true");

    [When("{string} lista los visitantes sin visitas")]
    public Task ListsVisitorsWithoutVisits(string name) => _client.SendAsync(_state.User(name), HttpMethod.Get, $"{VisitorsUrl}?hasVisits=false");

    [When("{string} lista los visitantes por más visitas")]
    public Task ListsVisitorsByMostVisits(string name) => _client.SendAsync(_state.User(name), HttpMethod.Get, $"{VisitorsUrl}?sort=MostVisits");

    [When("{string} consulta el resumen de visitantes")]
    public Task QueriesSummary(string name) => _client.SendAsync(_state.User(name), HttpMethod.Get, $"{VisitorsUrl}/summary");

    [Then("el registro de visitante no se encuentra")]
    public void RecordIsNotFound() => Assert.AreEqual(HttpStatusCode.NotFound, _state.Response!.StatusCode);

    [Then("{string} consulta su registro de visitante y ve:")]
    public async Task QueriesOwnRecordAndSees(string name, DataTable table)
    {
        await QueriesOwnRecord(name);

        var visitor = (await ReadDetailAsync()).Visitor;
        var expected = table.Rows[0];

        foreach (var column in table.Header)
            Assert.AreEqual(expected[column], ValueOf(visitor, column), $"Columna '{column}'.");
    }

    [Then("el primer visitante de la lista es {string}")]
    public async Task FirstVisitorIs(string name)
    {
        Assert.AreEqual(HttpStatusCode.OK, _state.Response!.StatusCode);
        Assert.AreEqual(name, (await _client.ReadAsync<PageOf<VisitorDto>>()).Items[0].Name);
    }

    [Then("el resumen tiene {int} visitantes, {int} nuevos y {int} sin visitas")]
    public async Task SummaryIs(int total, int newLastWeek, int withoutVisits)
    {
        Assert.AreEqual(HttpStatusCode.OK, _state.Response!.StatusCode);
        Assert.AreEqual(new VisitorsSummaryDto(total, newLastWeek, withoutVisits), await _client.ReadAsync<VisitorsSummaryDto>());
    }

    [Then("el visitante tiene {int} visita(s) y {int} inasistencia(s)")]
    public async Task VisitorHasVisits(int visits, int noShows)
    {
        var detail = await ReadDetailAsync();

        Assert.AreEqual(visits, detail.Visitor.VisitCount);
        Assert.HasCount(visits, detail.Visits);
        Assert.AreEqual(noShows, detail.Visitor.NoShowCount);
    }

    [Then("la visita del visitante está en estado {string}")]
    public async Task VisitorVisitIs(string status)
        => Assert.AreEqual(Vocabulary.VisitStatusOf(status).ToString(), (await ReadDetailAsync()).Visits.Single().Status);

    [Then("lo que sigue para el visitante es {string}")]
    public async Task NextStepIs(string status)
        => Assert.AreEqual(Vocabulary.VisitStatusOf(status).ToString(), (await ReadDetailAsync()).Visitor.NextStep?.Status);

    [Then("el visitante no tiene nada pendiente")]
    public async Task NothingPending() => Assert.IsNull((await ReadDetailAsync()).Visitor.NextStep);

    [Then("la actividad del visitante es:")]
    public async Task ActivityIs(DataTable table)
    {
        var activity = (await ReadDetailAsync()).Activity.Select(entry => entry.Action).ToList();

        Assert.AreEqual(string.Join(", ", table.Rows.Select(row => ActionOf(row["acción"]))), string.Join(", ", activity));
    }

    private async Task<VisitorDetailDto> ReadDetailAsync()
    {
        Assert.IsTrue(_state.Response!.IsSuccessStatusCode, await _state.Response.Content.ReadAsStringAsync());

        return await _client.ReadAsync<VisitorDetailDto>();
    }

    private static string ValueOf(VisitorDto visitor, string column) => column switch
    {
        "nombre" => visitor.Name,
        "correo" => visitor.Email,
        "teléfono" => visitor.Phone,
        _ => throw new ArgumentOutOfRangeException(nameof(column), column, null)
    };

    private static string ActionOf(string text) => text switch
    {
        "se registró" => "Registered",
        "cambió su nombre" => "NameChanged",
        "cambió su correo" => "EmailChanged",
        "cambió su teléfono" => "PhoneChanged",
        "pidió una visita" => "Requested",
        "propuso otras franjas" => "CounterProposed",
        "agendó la visita" => "Scheduled",
        "canceló la visita" => "Cancelled",
        "venció la visita" => "Expired",
        "realizó la visita" => "Completed",
        "no asistió" => "NoShow",
        _ => throw new ArgumentOutOfRangeException(nameof(text), text, "Acción de actividad desconocida.")
    };
}
