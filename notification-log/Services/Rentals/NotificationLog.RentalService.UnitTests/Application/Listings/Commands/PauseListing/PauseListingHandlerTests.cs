using NotificationLog.RentalService.Application.Listings.Commands.PauseListing;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.UnitTests.Application.Listings.Commands.PauseListing;

[TestClass]
[TestCategory("Application-Listings")]
public class PauseListingHandlerTests : ApplicationScenario
{
    private Listing _listing = null!;

    [TestMethod]
    public void TheOwnerPausesTheirListing() =>
        this.Given(_ => AListing(), "Dada una publicación publicada")
            .And(_ => OwnerSignsIn(), "Y que su propietario inicia sesión")
            .When(_ => Pauses(), "Cuando la pausa")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => StatusIs(ListingStatus.Paused), "Y la publicación queda pausada")
            .And(_ => IsSaved(_listing), "Y se guarda el cambio")
            .BDDfy("El propietario pausa su publicación");

    [TestMethod]
    public void AnotherOwnerCannotPauseTheListing() =>
        this.Given(_ => AListing(), "Dada una publicación publicada")
            .And(_ => AnotherOwnerSignsIn(), "Y que otro propietario inicia sesión")
            .When(_ => Pauses(), "Cuando la pausa")
            .Then(_ => IsForbidden(), "Entonces se rechaza por falta de permiso")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .BDDfy("Otro propietario no puede pausar la publicación");

    [TestMethod]
    public void AMissingListingIsNotFound() =>
        this.Given(_ => AMissingListing(), "Dada una publicación que no existe")
            .And(_ => OwnerSignsIn(), "Y que un propietario inicia sesión")
            .When(_ => Pauses(), "Cuando la pausa")
            .Then(_ => IsNotFound(), "Entonces la publicación no se encuentra")
            .BDDfy("No se puede pausar una publicación que no existe");

    private void AListing() => Exists(_listing = ListingFactory.InStatus(ListingStatus.Published));

    private void AMissingListing() => _listing = ListingFactory.InStatus(ListingStatus.Published);

    private Task Pauses()
        => TryAsync(() => Handler<PauseListingHandler>().HandleAsync(new PauseListingCommand(_listing.Id), User, CancellationToken.None));

    private void StatusIs(ListingStatus expected) => Assert.AreEqual(expected, _listing.Status);
}
