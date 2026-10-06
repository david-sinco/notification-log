using NotificationLog.RentalService.Api.Contracts.Listings;
using NotificationLog.RentalService.Api.Contracts.Moderation;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.IntegrationTests.StepDefinitions;

[Binding]
public sealed class ModerationStepDefinitions(RentalsClient client, ScenarioState state, ListingSetup setup)
{
    private readonly RentalsClient _client = client;
    private readonly ScenarioState _state = state;
    private readonly ListingSetup _setup = setup;

    private string ModerationUrl => $"/api/moderation/listings/{_state.ListingId}";

    [Given("una publicación de arriendo de {string} rechazada por {string}")]
    public async Task ARejectedListing(string owner, string reason)
    {
        await _setup.InStatusAsync(_state.User(owner), Operation.Rent, ListingStatus.InReview);

        await _client.SetUpAsync(
            _state.Staff, HttpMethod.Post, $"{ModerationUrl}/reject", new RejectListingRequest([Enum.Parse<RejectionReason>(reason)]));
    }

    [When("{string} aprueba la publicación")]
    public Task Approves(string name) => _client.SendAsync(_state.User(name), HttpMethod.Post, $"{ModerationUrl}/approve");

    [When("{string} rechaza la publicación por los motivos:")]
    public Task RejectsFor(string name, DataTable table)
        => RejectAsync(name, table.Rows.Select(row => Enum.Parse<RejectionReason>(row["motivo"])).ToList());

    [When("{string} rechaza la publicación sin motivos")]
    public Task RejectsWithoutReasons(string name) => RejectAsync(name, []);

    [When("{string} suspende la publicación con el motivo {string}")]
    public Task Suspends(string name, string reason)
        => _client.SendAsync(_state.User(name), HttpMethod.Post, $"{ModerationUrl}/suspend", new SuspendListingRequest(reason));

    [When("{string} restablece la publicación")]
    public Task Reinstates(string name) => _client.SendAsync(_state.User(name), HttpMethod.Post, $"{ModerationUrl}/reinstate");

    [When("{string} retira la publicación por moderación con el motivo {string}")]
    public Task WithdrawsAsModeration(string name, string reason)
        => _client.SendAsync(_state.User(name), HttpMethod.Post, $"{ModerationUrl}/withdraw", new WithdrawListingRequest(reason));

    private Task RejectAsync(string name, IReadOnlyList<RejectionReason> reasons)
        => _client.SendAsync(_state.User(name), HttpMethod.Post, $"{ModerationUrl}/reject", new RejectListingRequest(reasons));
}
