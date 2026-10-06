using NotificationLog.RentalService.Application.Listings.Commands.SubmitListingForReview;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.UnitTests.Application.Listings.Commands.SubmitListingForReview;

[TestClass]
public class SubmitListingForReviewHandlerTests : ApplicationScenario
{
    private Listing _listing = null!;

    [TestMethod]
    public void TheOwnerSubmitsTheirListing() =>
        this.Given(_ => AListing(), "Dada una publicación en borrador completa")
            .And(_ => OwnerSignsIn(), "Y que su propietario inicia sesión")
            .When(_ => Submits(), "Cuando la envía a revisión")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => StatusIs(ListingStatus.InReview), "Y la publicación queda en revisión")
            .And(_ => IsSaved(_listing), "Y se guarda el cambio")
            .BDDfy("El propietario envía a revisión su publicación");

    [TestMethod]
    public void AnotherOwnerCannotSubmitTheListing() =>
        this.Given(_ => AListing(), "Dada una publicación en borrador completa")
            .And(_ => AnotherOwnerSignsIn(), "Y que otro propietario inicia sesión")
            .When(_ => Submits(), "Cuando la envía a revisión")
            .Then(_ => IsForbidden(), "Entonces se rechaza por falta de permiso")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .BDDfy("Otro propietario no puede enviar a revisión la publicación");

    [TestMethod]
    public void AMissingListingIsNotFound() =>
        this.Given(_ => AMissingListing(), "Dada una publicación que no existe")
            .And(_ => OwnerSignsIn(), "Y que un propietario inicia sesión")
            .When(_ => Submits(), "Cuando la envía a revisión")
            .Then(_ => IsNotFound(), "Entonces la publicación no se encuentra")
            .BDDfy("No se puede enviar a revisión una publicación que no existe");

    private void AListing() => Exists(_listing = ListingFactory.InStatus(ListingStatus.Draft));

    private void AMissingListing() => _listing = ListingFactory.InStatus(ListingStatus.Draft);

    private Task Submits()
        => TryAsync(() => Handler<SubmitListingForReviewHandler>().HandleAsync(new SubmitListingForReviewCommand(_listing.Id), User, CancellationToken.None));

    private void StatusIs(ListingStatus expected) => Assert.AreEqual(expected, _listing.Status);
}
