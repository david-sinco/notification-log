using NotificationLog.RentalService.Api.Contracts.Listings;
using NotificationLog.RentalService.Api.Contracts.Moderation;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.IntegrationTests.Support;

public sealed class ListingSetup(RentalsApi api, RentalsClient client, ScenarioState state)
{
    public const int RequiredPhotos = 5;

    private readonly RentalsApi _api = api;
    private readonly RentalsClient _client = client;
    private readonly ScenarioState _state = state;

    public static UpdateListingDetailsRequest Details(string neighborhood = "Chapinero", int descriptionLength = 120)
        => new(
            PropertyType.Apartment, 68m, 2, 2, 1, 4, 5, true, 320_000,
            "Bogotá", neighborhood, "Calle 60 # 9-45", new string('a', descriptionLength));

    public async Task<Guid> DraftAsync(
        TestUser owner,
        Operation operation,
        string neighborhood = "Chapinero",
        bool withDetails = true,
        bool withPrice = true,
        int photos = RequiredPhotos)
    {
        var created = await _client.SetUpAsync<CreatedListingResponse>(
            owner, HttpMethod.Post, "/api/listings", new DraftListingRequest(owner.Id, operation));

        var url = $"/api/listings/{created.Id}";

        if (withDetails)
            await _client.SetUpAsync(owner, HttpMethod.Put, $"{url}/details", Details(neighborhood));

        if (withPrice)
            await _client.SetUpAsync(
                owner, HttpMethod.Patch, $"{url}/price", new ChangeListingPriceRequest(operation == Operation.Rent ? 1_800_000 : 250_000_000));

        for (var i = 0; i < photos; i++)
            await _client.SetUpPhotoAsync(owner, created.Id);

        _state.ListingId = created.Id;

        return created.Id;
    }

    public async Task<Guid> InStatusAsync(
        TestUser owner, Operation operation, ListingStatus status, string neighborhood = "Chapinero")
    {
        var id = await DraftAsync(owner, operation, neighborhood);
        var url = $"/api/listings/{id}";
        var moderation = $"/api/moderation/listings/{id}";

        if (status == ListingStatus.Draft)
            return id;

        await _client.SetUpAsync(owner, HttpMethod.Post, $"{url}/submit");

        if (status == ListingStatus.InReview)
            return id;

        await _client.SetUpAsync(_state.Staff, HttpMethod.Post, $"{moderation}/approve");

        switch (status)
        {
            case ListingStatus.Published:
                break;
            case ListingStatus.Paused:
                await _client.SetUpAsync(owner, HttpMethod.Post, $"{url}/pause");
                break;
            case ListingStatus.Suspended:
                await _client.SetUpAsync(
                    _state.Staff, HttpMethod.Post, $"{moderation}/suspend", new SuspendListingRequest("Denuncia por datos falsos"));
                break;
            case ListingStatus.Withdrawn:
                await _client.SetUpAsync(owner, HttpMethod.Post, $"{url}/withdraw", new WithdrawListingRequest("Decidí no arrendar"));
                break;
            case ListingStatus.Closed:
                await _client.SetUpAsync(
                    owner,
                    HttpMethod.Post,
                    $"{url}/close",
                    new CloseListingRequest(1_750_000, DateOnly.FromDateTime(_api.Clock.GetUtcNow().UtcDateTime)));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(status), status, "No hay preparación para ese estado.");
        }

        return id;
    }
}
