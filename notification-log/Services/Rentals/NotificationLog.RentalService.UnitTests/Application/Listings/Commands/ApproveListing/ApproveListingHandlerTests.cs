using NotificationLog.RentalService.Application.Listings.Commands.ApproveListing;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;
using NotificationLog.RentalService.Domain.Listings.Events;

namespace NotificationLog.RentalService.UnitTests.Application.Listings.Commands.ApproveListing;

[TestClass]
[TestCategory("Application-Listings")]
public class ApproveListingHandlerTests : ApplicationScenario
{
    private Listing _listing = null!;

    [TestMethod]
    public void AModeratorApprovesAListingInReview() =>
        this.Given(_ => AListing(), "Dada una publicación en revisión")
            .And(_ => ModeratorSignsIn(), "Y que un moderador inicia sesión")
            .And(_ => ItIs("2026-10-10 09:00"), "Y son las 2026-10-10 09:00")
            .When(_ => Approves(), "Cuando la aprueba")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => StatusIs(ListingStatus.Published), "Y la publicación queda publicada")
            .And(_ => ExpiresOn("2026-11-10 09:00"), "Y vence el 2026-11-10 09:00, un mes después de la hora actual")
            .And(_ => TheModeratorIsRecorded(), "Y queda registrado el moderador que la aprobó")
            .And(_ => IsSaved(_listing), "Y se guarda el cambio")
            .BDDfy("Un moderador aprueba una publicación en revisión");

    [TestMethod]
    public void AMissingListingIsNotFound() =>
        this.Given(_ => AMissingListing(), "Dada una publicación que no existe")
            .And(_ => ModeratorSignsIn(), "Y que un moderador inicia sesión")
            .When(_ => Approves(), "Cuando la aprueba")
            .Then(_ => IsNotFound(), "Entonces la publicación no se encuentra")
            .BDDfy("No se puede aprobar una publicación que no existe");

    private void AListing() => Exists(_listing = ListingFactory.InStatus(ListingStatus.InReview));

    private void AMissingListing() => _listing = ListingFactory.InStatus(ListingStatus.InReview);

    private Task Approves()
        => TryAsync(() => Handler<ApproveListingHandler>()
            .HandleAsync(new ApproveListingCommand(_listing.Id), User, CancellationToken.None));

    private void StatusIs(ListingStatus expected) => Assert.AreEqual(expected, _listing.Status);

    private void ExpiresOn(string moment) => Assert.AreEqual(Clock.At(moment), _listing.ExpiresAt);

    private void TheModeratorIsRecorded()
        => Assert.AreEqual(ListingFactory.Moderator, _listing.DomainEvents.OfType<ListingApproved>().Single().ModeratorId);
}
