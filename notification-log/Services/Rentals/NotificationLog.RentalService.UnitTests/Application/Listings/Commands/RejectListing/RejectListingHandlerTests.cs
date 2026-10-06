using NotificationLog.RentalService.Application.Listings.Commands.RejectListing;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;
using NotificationLog.RentalService.Domain.Listings.Events;

namespace NotificationLog.RentalService.UnitTests.Application.Listings.Commands.RejectListing;

[TestClass]
public class RejectListingHandlerTests : ApplicationScenario
{
    private Listing _listing = null!;

    [TestMethod]
    public void AModeratorRejectsAListingInReview() =>
        this.Given(_ => AListing(), "Dada una publicación en revisión")
            .And(_ => ModeratorSignsIn(), "Y que un moderador inicia sesión")
            .When(_ => RejectsFor(RejectionReason.LowQualityPhotos), "Cuando la rechaza por fotos de baja calidad")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => StatusIs(ListingStatus.Draft), "Y la publicación vuelve a borrador")
            .And(_ => TheModeratorIsRecorded(), "Y queda registrado el moderador que la rechazó")
            .And(_ => IsSaved(_listing), "Y se guarda el cambio")
            .BDDfy("Un moderador rechaza una publicación en revisión");

    [TestMethod]
    public void AtLeastOneReasonIsRequired() =>
        this.Given(_ => AListing(), "Dada una publicación en revisión")
            .And(_ => ModeratorSignsIn(), "Y que un moderador inicia sesión")
            .When(_ => RejectsFor(), "Cuando la rechaza sin motivos")
            .Then(_ => FailsValidationOn("Reasons"), "Entonces la validación falla en los motivos")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .BDDfy("Rechazar exige al menos un motivo");

    [TestMethod]
    public void AMissingListingIsNotFound() =>
        this.Given(_ => AMissingListing(), "Dada una publicación que no existe")
            .And(_ => ModeratorSignsIn(), "Y que un moderador inicia sesión")
            .When(_ => RejectsFor(RejectionReason.LowQualityPhotos), "Cuando la rechaza")
            .Then(_ => IsNotFound(), "Entonces la publicación no se encuentra")
            .BDDfy("No se puede rechazar una publicación que no existe");

    private void AListing() => Exists(_listing = ListingFactory.InStatus(ListingStatus.InReview));

    private void AMissingListing() => _listing = ListingFactory.InStatus(ListingStatus.InReview);

    private Task RejectsFor(params RejectionReason[] reasons)
        => TryAsync(() => Handler<RejectListingHandler>()
            .HandleAsync(new RejectListingCommand(_listing.Id, reasons), User, CancellationToken.None));

    private void StatusIs(ListingStatus expected) => Assert.AreEqual(expected, _listing.Status);

    private void TheModeratorIsRecorded()
        => Assert.AreEqual(ListingFactory.Moderator, _listing.DomainEvents.OfType<ListingRejected>().Single().ModeratorId);
}
