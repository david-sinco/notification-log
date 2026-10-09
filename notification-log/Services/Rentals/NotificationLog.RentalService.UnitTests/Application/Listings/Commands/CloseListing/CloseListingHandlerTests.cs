using NotificationLog.RentalService.Application.Listings.Commands.CloseListing;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.UnitTests.Application.Listings.Commands.CloseListing;

[TestClass]
[TestCategory("Application-Listings")]
public class CloseListingHandlerTests : ApplicationScenario
{
    private static readonly DateOnly Today = new(2026, 10, 5);

    private Listing _listing = null!;

    [TestMethod]
    public void TheOwnerClosesTheirListing() =>
        this.Given(_ => AListing(), "Dada una publicación publicada")
            .And(_ => OwnerSignsIn(), "Y que su propietario inicia sesión")
            .When(_ => ClosesFor(1_750_000, Today), "Cuando la cierra por 1750000, firmada hoy")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => StatusIs(ListingStatus.Closed), "Y la publicación queda cerrada")
            .And(_ => IsSaved(_listing), "Y se guarda el cambio")
            .BDDfy("El propietario cierra su publicación");

    [TestMethod]
    public void TheSignatureDateIsCheckedAgainstTheClock() =>
        this.Given(_ => AListing(), "Dada una publicación publicada")
            .And(_ => OwnerSignsIn(), "Y que su propietario inicia sesión")
            .When(_ => ClosesFor(1_750_000, Today.AddDays(1)), "Cuando la cierra con fecha de firma de mañana")
            .Then(_ => IsRejectedWith("La fecha de firma no puede estar en el futuro."),
                "Entonces se rechaza: La fecha de firma no puede estar en el futuro.")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .BDDfy("La fecha de firma se compara con la fecha actual");

    [TestMethod]
    public void TheFinalPriceMustBeGreaterThanZero() =>
        this.Given(_ => AListing(), "Dada una publicación publicada")
            .And(_ => OwnerSignsIn(), "Y que su propietario inicia sesión")
            .When(_ => ClosesFor(0, Today), "Cuando la cierra por 0")
            .Then(_ => FailsValidationOn("FinalPrice"), "Entonces la validación falla en el valor final")
            .BDDfy("El valor final debe ser mayor que cero");

    [TestMethod]
    public void AnotherOwnerCannotCloseTheListing() =>
        this.Given(_ => AListing(), "Dada una publicación publicada")
            .And(_ => AnotherOwnerSignsIn(), "Y que otro propietario inicia sesión")
            .When(_ => ClosesFor(1_750_000, Today), "Cuando la cierra")
            .Then(_ => IsForbidden(), "Entonces se rechaza por falta de permiso")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .BDDfy("Otro propietario no puede cerrar la publicación");

    [TestMethod]
    public void AMissingListingIsNotFound() =>
        this.Given(_ => AMissingListing(), "Dada una publicación que no existe")
            .And(_ => OwnerSignsIn(), "Y que un propietario inicia sesión")
            .When(_ => ClosesFor(1_750_000, Today), "Cuando la cierra")
            .Then(_ => IsNotFound(), "Entonces la publicación no se encuentra")
            .BDDfy("No se puede cerrar una publicación que no existe");

    private void AListing() => Exists(_listing = ListingFactory.InStatus(ListingStatus.Published));

    private void AMissingListing() => _listing = ListingFactory.InStatus(ListingStatus.Published);

    private Task ClosesFor(long finalPrice, DateOnly signedOn)
        => TryAsync(() => Handler<CloseListingHandler>()
            .HandleAsync(new CloseListingCommand(_listing.Id, finalPrice, signedOn), User, CancellationToken.None));

    private void StatusIs(ListingStatus expected) => Assert.AreEqual(expected, _listing.Status);
}
