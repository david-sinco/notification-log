using NotificationLog.RentalService.Api.Contracts.Visits;
using NotificationLog.RentalService.Application.Visits.Queries.Dtos;
using NotificationLog.RentalService.Domain.Visits.Enums;

namespace NotificationLog.RentalService.IntegrationTests.StepDefinitions;

[Binding]
public sealed class VisitStepDefinitions(RentalsApi api, RentalsClient client, ScenarioState state)
{
    private const string VisitsUrl = "/api/visits";
    private const string DefaultSlot = "2026-10-07 10:00";

    private readonly RentalsApi _api = api;
    private readonly RentalsClient _client = client;
    private readonly ScenarioState _state = state;

    private string VisitUrl => $"{VisitsUrl}/{_state.VisitId}";

    [Given("que {string} pidió una visita a la publicación de {string} para el {string}")]
    public async Task HasRequestedAVisitFor(string visitor, string host, string slot)
        => _state.VisitId = (await _client.SetUpAsync<CreatedVisitResponse>(
            _state.User(visitor), HttpMethod.Post, VisitsUrl, Request([slot]))).Id;

    [Given("que {string} pidió una visita a la publicación de {string} proponiendo las franjas:")]
    public async Task HasRequestedAVisitWith(string visitor, string host, DataTable table)
        => _state.VisitId = (await _client.SetUpAsync<CreatedVisitResponse>(
            _state.User(visitor), HttpMethod.Post, VisitsUrl, Request(SlotsOf(table)))).Id;

    [Given("una visita de {string} a la publicación de {string} agendada para el {string}")]
    public async Task AScheduledVisit(string visitor, string host, string slot)
    {
        await HasRequestedAVisitFor(visitor, host, slot);
        await HasScheduledTheVisit(host, slot);
    }

    [Given("una visita de {string} a la publicación de {string} cancelada por {string}")]
    public async Task ACancelledVisit(string visitor, string host, string cancelledBy)
    {
        await HasRequestedAVisitFor(visitor, host, DefaultSlot);

        await _client.SetUpAsync(
            _state.User(cancelledBy), HttpMethod.Post, $"{VisitUrl}/cancel", new CancelVisitRequest("El inmueble ya no está disponible"));
    }

    [Given("que {string} agendó la visita para el {string}")]
    public Task HasScheduledTheVisit(string name, string slot)
        => _client.SetUpAsync(
            _state.User(name), HttpMethod.Post, $"{VisitUrl}/schedule", new ScheduleVisitRequest(ColombiaTime.Parse(slot)));

    [When("{string} pide una visita a la publicación de {string} para el {string}")]
    public Task RequestsAVisitFor(string visitor, string host, string slot) => RequestAsync(visitor, [slot]);

    [When("{string} pide una visita a la publicación de {string} proponiendo las franjas:")]
    public Task RequestsAVisitWith(string visitor, string host, DataTable table) => RequestAsync(visitor, SlotsOf(table));

    [When("{string} agenda la visita para el {string}")]
    public Task Schedules(string name, string slot)
        => _client.SendAsync(
            _state.User(name), HttpMethod.Post, $"{VisitUrl}/schedule", new ScheduleVisitRequest(ColombiaTime.Parse(slot)));

    [When("{string} propone otras franjas:")]
    public Task CounterProposes(string name, DataTable table)
        => _client.SendAsync(
            _state.User(name),
            HttpMethod.Post,
            $"{VisitUrl}/counter-proposal",
            new CounterProposeVisitRequest(SlotsOf(table).Select(ColombiaTime.Parse).ToList()));

    [When("{string} cancela la visita con el motivo {string}")]
    public Task Cancels(string name, string reason)
        => _client.SendAsync(_state.User(name), HttpMethod.Post, $"{VisitUrl}/cancel", new CancelVisitRequest(reason));

    [When("{string} marca la visita como realizada")]
    public Task MarksCompleted(string name) => _client.SendAsync(_state.User(name), HttpMethod.Post, $"{VisitUrl}/complete");

    [When("{string} marca que el visitante no asistió")]
    public Task MarksNoShow(string name) => _client.SendAsync(_state.User(name), HttpMethod.Post, $"{VisitUrl}/no-show");

    [When("{string} consulta la visita")]
    public Task QueriesTheVisit(string name) => _client.SendAsync(_state.User(name), HttpMethod.Get, VisitUrl);

    [When("{string} lista las visitas")]
    public Task ListsVisits(string name) => _client.SendAsync(_state.User(name), HttpMethod.Get, VisitsUrl);

    [Then("la visita queda en estado {string}")]
    public async Task VisitStatusIs(string status)
    {
        Assert.IsTrue(_state.Response!.IsSuccessStatusCode, await _state.Response.Content.ReadAsStringAsync());
        Assert.AreEqual(Vocabulary.VisitStatusOf(status).ToString(), (await VisitAsync()).Status);
    }

    [Then("la visita tiene {int} franjas propuestas")]
    public async Task VisitHasProposedSlots(int count) => Assert.HasCount(count, (await VisitAsync()).ProposedSlots);

    [Then("el plazo para responder vence el {string}")]
    public async Task DeadlineIs(string moment) => Assert.AreEqual(ColombiaTime.Parse(moment), (await VisitAsync()).RespondBy);

    [Then("la visita está agendada de {string} a {string}")]
    public async Task IsScheduledBetween(string start, string end)
    {
        var visit = await VisitAsync();

        Assert.AreEqual(ColombiaTime.Parse(start), visit.ScheduledStartsAt);
        Assert.AreEqual(ColombiaTime.Parse(end), visit.ScheduledEndsAt);
    }

    [Then("el historial de la visita tiene {int} entradas")]
    public async Task HistoryHas(int entries) => Assert.HasCount(entries, (await VisitAsync()).History);

    [Then("la visita fue cancelada por el anfitrión")]
    public async Task WasCancelledByTheHost()
        => Assert.AreEqual(nameof(VisitParty.Host), (await VisitAsync()).CancelledBy);

    [Then("la visita fue cancelada por el anfitrión con el motivo {string}")]
    public async Task WasCancelledByTheHostBecause(string reason)
    {
        var visit = await VisitAsync();

        Assert.AreEqual(nameof(VisitParty.Host), visit.CancelledBy);
        Assert.AreEqual(reason, visit.CancellationReason);
    }

    [Then("la cancelación es tardía")]
    public async Task CancellationIsLate() => Assert.IsTrue((await VisitAsync()).IsLateCancellation);

    [Then("la cancelación no es tardía")]
    public async Task CancellationIsNotLate() => Assert.IsFalse((await VisitAsync()).IsLateCancellation);

    [Then("se notifica {string} a {string}")]
    public void IsNotified(string eventKey, string recipient)
        => Assert.IsTrue(
            _api.Notifications.Sent.Any(sent =>
                sent.EventKey == eventKey
                && sent.RecipientId == _state.User(recipient).Id
                && sent.Data["visit_id"] == _state.VisitId.ToString()),
            $"No se notificó '{eventKey}' a {recipient}. Enviadas: {string.Join(", ", _api.Notifications.Sent.Select(sent => sent.EventKey))}");

    [Then("no se envía ninguna notificación nueva")]
    public void NoNewNotificationIsSent()
        => Assert.AreEqual(_state.NotificationsBeforeLastRequest, _api.Notifications.Sent.Count);

    private async Task RequestAsync(string visitor, IReadOnlyList<string> slots)
    {
        var response = await _client.SendAsync(_state.User(visitor), HttpMethod.Post, VisitsUrl, Request(slots));

        if (response.IsSuccessStatusCode)
            _state.VisitId = (await _client.ReadAsync<CreatedVisitResponse>()).Id;
    }

    private RequestVisitRequest Request(IReadOnlyList<string> slots)
        => new(_state.ListingId, slots.Select(ColombiaTime.Parse).ToList());

    private static IReadOnlyList<string> SlotsOf(DataTable table) => table.Rows.Select(row => row["franja"]).ToList();

    private Task<VisitDto> VisitAsync() => _client.QueryAsync<VisitDto>(VisitUrl);
}
