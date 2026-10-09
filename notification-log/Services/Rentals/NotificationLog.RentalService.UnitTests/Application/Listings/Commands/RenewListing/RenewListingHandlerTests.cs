using NotificationLog.RentalService.Application.Listings.Commands.RenewListing;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.UnitTests.Application.Listings.Commands.RenewListing;

[TestClass]
[TestCategory("Application-Listings")]
public class RenewListingHandlerTests : ApplicationScenario
{
    private Listing _listing = null!;

    [TestMethod]
    public void TheOwnerRenewsTheirListing() =>
        this.Given(_ => AListing(), "Dada una publicación vencida")
            .And(_ => OwnerSignsIn(), "Y que su propietario inicia sesión")
            .When(_ => Renews(), "Cuando la renueva")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => StatusIs(ListingStatus.Published), "Y la publicación queda publicada")
            .And(_ => IsSaved(_listing), "Y se guarda el cambio")
            .BDDfy("El propietario renueva su publicación");

    [TestMethod]
    public void AnotherOwnerCannotRenewTheListing() =>
        this.Given(_ => AListing(), "Dada una publicación vencida")
            .And(_ => AnotherOwnerSignsIn(), "Y que otro propietario inicia sesión")
            .When(_ => Renews(), "Cuando la renueva")
            .Then(_ => IsForbidden(), "Entonces se rechaza por falta de permiso")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .BDDfy("Otro propietario no puede renovar la publicación");

    [TestMethod]
    public void AMissingListingIsNotFound() =>
        this.Given(_ => AMissingListing(), "Dada una publicación que no existe")
            .And(_ => OwnerSignsIn(), "Y que un propietario inicia sesión")
            .When(_ => Renews(), "Cuando la renueva")
            .Then(_ => IsNotFound(), "Entonces la publicación no se encuentra")
            .BDDfy("No se puede renovar una publicación que no existe");

    private void AListing() => Exists(_listing = ListingFactory.InStatus(ListingStatus.Expired));

    private void AMissingListing() => _listing = ListingFactory.InStatus(ListingStatus.Expired);

    private Task Renews()
        => TryAsync(() => Handler<RenewListingHandler>().HandleAsync(new RenewListingCommand(_listing.Id), User, CancellationToken.None));

    private void StatusIs(ListingStatus expected) => Assert.AreEqual(expected, _listing.Status);
}
