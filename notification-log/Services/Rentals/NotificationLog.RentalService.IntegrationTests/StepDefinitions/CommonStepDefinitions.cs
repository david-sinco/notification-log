using System.Net;
using System.Text.Json;
using Domain.Shared.Authorization;
using Timestamp = Google.Protobuf.WellKnownTypes.Timestamp;
using NotificationLog.Contracts.Identity;

namespace NotificationLog.RentalService.IntegrationTests.StepDefinitions;

[Binding]
public sealed class CommonStepDefinitions(RentalsApi api, RentalsClient client, ScenarioState state)
{
    private readonly RentalsApi _api = api;
    private readonly RentalsClient _client = client;
    private readonly ScenarioState _state = state;

    [Given("que ahora son las {string} hora de Colombia")]
    [When("el reloj avanza hasta las {string}")]
    public void SetTheClock(string moment) => _api.Clock.Set(ColombiaTime.Parse(moment));

    [When("pasan {int} horas")]
    public void HoursPass(int hours) => _api.Clock.Advance(TimeSpan.FromHours(hours));

    [When("pasan {int} días")]
    public void DaysPass(int days) => _api.Clock.Advance(TimeSpan.FromDays(days));

    [Given("el usuario {string} con rol {word}")]
    public Task AddUser(string name, string role)
    {
        var user = _state.AddUser(name, Enum.Parse<UserRole>(role));

        return _api.PublishAsync(new UserCreated
        {
            EventId = Guid.NewGuid().ToString(),
            OccurredAt = Timestamp.FromDateTimeOffset(_api.Clock.GetUtcNow()),
            SchemaVersion = 1,
            UserId = user.Id.ToString(),
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            Role = user.Role.ToString()
        });
    }

    [Then("la solicitud se acepta")]
    public async Task RequestIsAccepted()
        => Assert.IsTrue(_state.Response!.IsSuccessStatusCode, await _state.Response.Content.ReadAsStringAsync());

    [Then("la solicitud se rechaza por falta de autenticación")]
    public void RequestIsUnauthenticated() => StatusIs(HttpStatusCode.Unauthorized);

    [Then("la solicitud se rechaza por permisos")]
    public void RequestIsForbidden() => StatusIs(HttpStatusCode.Forbidden);

    [Then("la solicitud se rechaza por validación")]
    public void RequestFailsValidation() => StatusIs(HttpStatusCode.BadRequest);

    [Then("la solicitud se rechaza por validación con el mensaje {string}")]
    public async Task RequestFailsValidationWith(string message)
    {
        StatusIs(HttpStatusCode.BadRequest);
        Assert.AreEqual(message, await _client.ReadErrorAsync());
    }

    [Then("la solicitud se rechaza por regla de negocio")]
    public void RequestBreaksABusinessRule() => StatusIs(HttpStatusCode.UnprocessableEntity);

    [Then("la solicitud se rechaza por regla de negocio con el mensaje {string}")]
    public async Task RequestBreaksABusinessRuleWith(string message)
    {
        StatusIs(HttpStatusCode.UnprocessableEntity);
        Assert.AreEqual(message, await _client.ReadErrorAsync());
    }

    [Then("la lista contiene {int} {word}")]
    public async Task ListContains(int count, string _)
    {
        StatusIs(HttpStatusCode.OK);

        var page = await _client.ReadAsync<PageOf<JsonElement>>();

        Assert.AreEqual(count, page.TotalCount);
        Assert.HasCount(count, page.Items);
    }

    private void StatusIs(HttpStatusCode expected) => Assert.AreEqual(expected, _state.Response!.StatusCode);
}
