using NotificationLog.RentalService.Application.Listings.Commands.SuspendListing;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.UnitTests.Application.Listings.Commands.SuspendListing;

[TestClass]
[TestCategory("Application-Listings")]
public class SuspendListingHandlerTests : ApplicationScenario
{
    private Listing _listing = null!;

    [TestMethod]
    public void AModeratorSuspendsAPublishedListing() =>
        this.Given(_ => AListing(), "Dada una publicación publicada")
            .And(_ => ModeratorSignsIn(), "Y que un moderador inicia sesión")
            .When(_ => SuspendsBecause("Denuncia por datos falsos"), "Cuando la suspende con el motivo «Denuncia por datos falsos»")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => StatusIs(ListingStatus.Suspended), "Y la publicación queda suspendida")
            .And(_ => IsSaved(_listing), "Y se guarda el cambio")
            .BDDfy("Un moderador suspende una publicación publicada");

    [TestMethod]
    public void AReasonIsRequired() =>
        this.Given(_ => AListing(), "Dada una publicación publicada")
            .And(_ => ModeratorSignsIn(), "Y que un moderador inicia sesión")
            .When(_ => SuspendsBecause(string.Empty), "Cuando la suspende sin motivo")
            .Then(_ => FailsValidationOn("Reason"), "Entonces la validación falla en el motivo")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .BDDfy("Suspender exige un motivo");

    [TestMethod]
    public void AMissingListingIsNotFound() =>
        this.Given(_ => AMissingListing(), "Dada una publicación que no existe")
            .And(_ => ModeratorSignsIn(), "Y que un moderador inicia sesión")
            .When(_ => SuspendsBecause("Denuncia por datos falsos"), "Cuando la suspende")
            .Then(_ => IsNotFound(), "Entonces la publicación no se encuentra")
            .BDDfy("No se puede suspender una publicación que no existe");

    private void AListing() => Exists(_listing = ListingFactory.InStatus(ListingStatus.Published));

    private void AMissingListing() => _listing = ListingFactory.InStatus(ListingStatus.Published);

    private Task SuspendsBecause(string reason)
        => TryAsync(() => Handler<SuspendListingHandler>()
            .HandleAsync(new SuspendListingCommand(_listing.Id, reason), User, CancellationToken.None));

    private void StatusIs(ListingStatus expected) => Assert.AreEqual(expected, _listing.Status);
}
