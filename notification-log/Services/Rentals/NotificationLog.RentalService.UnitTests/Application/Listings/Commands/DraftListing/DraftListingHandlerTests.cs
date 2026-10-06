using Domain.Shared.Authorization;
using NotificationLog.RentalService.Application.Listings.Commands.DraftListing;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;
using NSubstitute;

namespace NotificationLog.RentalService.UnitTests.Application.Listings.Commands.DraftListing;

[TestClass]
public class DraftListingHandlerTests : ApplicationScenario
{
    private Guid _listingId;

    private UserRole Role { get; set; }
    private bool Self { get; set; }
    private string Result { get; set; } = string.Empty;

    [TestMethod]
    public void AnOwnerDraftsAListingInTheirOwnName() =>
        this.Given(_ => ARegisteredOwner(), "Dado un propietario registrado")
            .And(_ => AUser(UserRole.Propietario, true), "Y que es él quien inicia sesión")
            .When(_ => DraftsAListingForTheOwner(), "Cuando crea una publicación a su nombre")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => TheDraftIsSavedForTheOwner(), "Y se guarda un borrador a nombre del propietario, creado por el usuario")
            .BDDfy("Un propietario crea una publicación a su nombre");

    [TestMethod]
    public void OnlyStaffCanDraftOnBehalfOfAnOwner() =>
        this.Given(_ => ARegisteredOwner(), "Dado un propietario registrado")
            .And(_ => AUser(Role, Self), "Y un usuario con rol <role> que es ese propietario: <self>")
            .When(_ => DraftsAListingForTheOwner(), "Cuando crea una publicación a nombre del propietario")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("role", "self", "result")
            {
                { UserRole.Propietario, true, Accepted },
                { UserRole.Propietario, false, "Un propietario solo puede crear publicaciones a su nombre." },
                { UserRole.Moderador, false, Accepted },
                { UserRole.Administrador, false, Accepted },
            })
            .BDDfy("Solo un administrador o un moderador crean publicaciones a nombre de otro");

    [TestMethod]
    public void TheOwnerMustBeRegistered() =>
        this.Given(_ => AUser(UserRole.Moderador, false), "Dado un moderador")
            .When(_ => DraftsAListingForTheOwner(), "Cuando crea una publicación a nombre de un propietario que no existe")
            .Then(_ => IsRejectedWith("El propietario no está registrado."), "Entonces se rechaza: El propietario no está registrado.")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .BDDfy("El propietario de la publicación debe estar registrado");

    [TestMethod]
    public void TheCommandNeedsAnOwner() =>
        this.Given(_ => AUser(UserRole.Moderador, false), "Dado un moderador")
            .When(_ => DraftsAListingWithoutOwner(), "Cuando crea una publicación sin indicar el propietario")
            .Then(_ => FailsValidationOn("OwnerId"), "Entonces la validación falla en el propietario")
            .BDDfy("La publicación exige indicar el propietario");

    private void ARegisteredOwner() => Exists(OwnerFactory.Natural(ListingFactory.Owner));

    private void AUser(UserRole role, bool isTheOwner) => UserIs(role, isTheOwner ? ListingFactory.Owner : Guid.NewGuid());

    private Task DraftsAListingForTheOwner() => Draft(ListingFactory.Owner);

    private Task DraftsAListingWithoutOwner() => Draft(Guid.Empty);

    private Task Draft(Guid owner)
        => TryAsync(async () => _listingId = await Handler<DraftListingHandler>()
            .HandleAsync(new DraftListingCommand(owner, Operation.Rent), User, CancellationToken.None));

    private void TheDraftIsSavedForTheOwner()
    {
        Listings.Received(1).AppendAsync(
            Arg.Is<Listing>(listing =>
                listing.Id == _listingId
                && listing.OwnerId == ListingFactory.Owner
                && listing.CreatedBy == ListingFactory.Owner
                && listing.Status == ListingStatus.Draft),
            Arg.Any<CancellationToken>());
        IsSaved();
    }
}
