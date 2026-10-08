using Domain.Shared.Authorization;

namespace NotificationLog.RentalService.IntegrationTests.Support;

public sealed class ScenarioState
{
    private readonly Dictionary<string, TestUser> _users = [];
    private readonly Dictionary<string, Guid> _owners = [];
    private int _documents;
    private int _phones;

    public TestUser Staff { get; } = new("Moderación interna", Guid.NewGuid(), UserRole.Moderador);

    public HttpResponseMessage? Response { get; set; }
    public int NotificationsBeforeLastRequest { get; set; }

    public Guid OwnerId { get; set; }
    public Guid ListingId { get; set; }
    public Guid VisitId { get; set; }
    public IReadOnlyList<string> PhotosBeforeLastChange { get; set; } = [];

    public TestUser AddUser(string name, UserRole role) => _users[name] = new TestUser(name, Guid.NewGuid(), role);

    public TestUser User(string name)
        => _users.TryGetValue(name, out var user)
            ? user
            : throw new InvalidOperationException($"El escenario no declara al usuario '{name}'.");

    public void AddOwner(string name, Guid ownerId) => _owners[name] = ownerId;

    public Guid OwnerOf(string name)
        => _owners.TryGetValue(name, out var ownerId)
            ? ownerId
            : throw new InvalidOperationException($"El escenario no registra un propietario para '{name}'.");

    public string NextDocument() => $"5212{++_documents:0000}";

    public string NextPhone() => $"300{++_phones:0000000}";
}
