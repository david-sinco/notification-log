using System.Net;
using NotificationLog.Web.Api;
using NotificationLog.Web.Api.Rentals.Identity;

namespace NotificationLog.Web.Components.Rentals;

public enum RentalsActorKind
{
    User,
    Advisor,
    Moderator,
    Custom
}

public sealed record RentalsActorOption(Guid Id, RentalsActorKind Kind, string Label, Guid? PersonId = null);

public sealed class RentalsActor
{
    public static readonly RentalsActorOption TestModerator =
        new(new Guid("7e57a11d-0000-4000-8000-000000000001"), RentalsActorKind.Moderator, "Moderador de pruebas");

    public RentalsActorOption? Current { get; private set; }

    public Guid? Id => Current?.Id;

    public event Action? Changed;

    public void Set(RentalsActorOption? option)
    {
        Current = option;
        Changed?.Invoke();
    }

    public Guid Require() =>
        Current?.Id ?? throw new ApiException(
            HttpStatusCode.BadRequest,
            "Falta elegir con quién actuar",
            "Elegí un usuario, asesor o moderador en la barra «Actuando como».");

    public bool Is(Guid id) => Current?.Id == id;

    public static IReadOnlyList<RentalsActorOption> OptionsFrom(IdentitySnapshotDto identity) =>
    [
        .. identity.People
            .Where(p => p.UserId is not null)
            .Select(p => new RentalsActorOption(
                p.UserId!.Value,
                RentalsActorKind.User,
                $"Usuario {RentalsFormat.Short(p.UserId.Value)} · persona {RentalsFormat.Short(p.Id)}",
                p.Id)),
        .. identity.Advisors
            .Select(a => new RentalsActorOption(a.Id, RentalsActorKind.Advisor, $"Asesor {RentalsFormat.Short(a.Id)}")),
        TestModerator
    ];
}
