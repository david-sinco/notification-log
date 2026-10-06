using NotificationLog.RentalService.Application.Listings.Commands.ChangeListingPrice;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.UnitTests.Application.Listings.Commands.ChangeListingPrice;

[TestClass]
public class ChangeListingPriceHandlerTests : ApplicationScenario
{
    private Listing _listing = null!;

    [TestMethod]
    public void TheOwnerChangesThePriceOfTheirListing() =>
        this.Given(_ => AListing(), "Dada una publicación en borrador")
            .And(_ => OwnerSignsIn(), "Y que su propietario inicia sesión")
            .When(_ => ChangesThePriceTo(2_000_000), "Cuando fija el precio en 2000000")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => PriceIs(2_000_000), "Y el precio queda en 2000000")
            .And(_ => IsSaved(_listing), "Y se guarda el cambio")
            .BDDfy("El propietario cambia el precio de su publicación");

    [TestMethod]
    public void ThePriceMustBeGreaterThanZero() =>
        this.Given(_ => AListing(), "Dada una publicación en borrador")
            .And(_ => OwnerSignsIn(), "Y que su propietario inicia sesión")
            .When(_ => ChangesThePriceTo(0), "Cuando fija el precio en 0")
            .Then(_ => FailsValidationOn("Price"), "Entonces la validación falla en el precio")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .BDDfy("El precio debe ser mayor que cero");

    [TestMethod]
    public void AnotherOwnerCannotChangeThePrice() =>
        this.Given(_ => AListing(), "Dada una publicación en borrador")
            .And(_ => AnotherOwnerSignsIn(), "Y que otro propietario inicia sesión")
            .When(_ => ChangesThePriceTo(2_000_000), "Cuando fija el precio en 2000000")
            .Then(_ => IsForbidden(), "Entonces se rechaza por falta de permiso")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .BDDfy("Otro propietario no puede cambiar el precio de la publicación");

    [TestMethod]
    public void AMissingListingIsNotFound() =>
        this.Given(_ => AMissingListing(), "Dada una publicación que no existe")
            .And(_ => OwnerSignsIn(), "Y que un propietario inicia sesión")
            .When(_ => ChangesThePriceTo(2_000_000), "Cuando fija el precio en 2000000")
            .Then(_ => IsNotFound(), "Entonces la publicación no se encuentra")
            .BDDfy("No se puede cambiar el precio de una publicación que no existe");

    private void AListing() => Exists(_listing = ListingFactory.InStatus(ListingStatus.Draft));

    private void AMissingListing() => _listing = ListingFactory.InStatus(ListingStatus.Draft);

    private Task ChangesThePriceTo(long price)
        => TryAsync(() => Handler<ChangeListingPriceHandler>()
            .HandleAsync(new ChangeListingPriceCommand(_listing.Id, price), User, CancellationToken.None));

    private void PriceIs(long expected) => Assert.AreEqual(expected, _listing.Price?.Amount);
}
