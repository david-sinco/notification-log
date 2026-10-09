using NotificationLog.RentalService.Application.Listings.Commands.ReinstateListing;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.UnitTests.Application.Listings.Commands.ReinstateListing;

[TestClass]
[TestCategory("Application-Listings")]
public class ReinstateListingHandlerTests : ApplicationScenario
{
    private Listing _listing = null!;

    private string Moment { get; set; } = string.Empty;
    private ListingStatus Becomes { get; set; }

    [TestMethod]
    public void AModeratorReinstatesASuspendedListing() =>
        this.Given(_ => AListing(), "Dada una publicación suspendida")
            .And(_ => ModeratorSignsIn(), "Y que un moderador inicia sesión")
            .When(_ => Reinstates(), "Cuando la restablece")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => StatusIs(ListingStatus.Published), "Y la publicación vuelve a estar publicada")
            .And(_ => IsSaved(_listing), "Y se guarda el cambio")
            .BDDfy("Un moderador restablece una publicación suspendida");

    [TestMethod]
    public void TheValidityIsCheckedAgainstTheClock() =>
        this.Given(_ => AListing(), "Dada una publicación suspendida que vence el 2026-11-05 09:00")
            .And(_ => ModeratorSignsIn(), "Y que un moderador inicia sesión")
            .And(_ => ItIs(Moment), "Y son las <moment>")
            .When(_ => Reinstates(), "Cuando la restablece")
            .Then(_ => StatusIs(Becomes), "Entonces la publicación queda en estado <becomes>")
            .WithExamples(new ExampleTable("moment", "becomes")
            {
                { "2026-11-05 08:59", ListingStatus.Published },
                { "2026-11-05 09:00", ListingStatus.Expired },
            })
            .BDDfy("Al restablecer, la vigencia se compara con la hora actual");

    [TestMethod]
    public void AMissingListingIsNotFound() =>
        this.Given(_ => AMissingListing(), "Dada una publicación que no existe")
            .And(_ => ModeratorSignsIn(), "Y que un moderador inicia sesión")
            .When(_ => Reinstates(), "Cuando la restablece")
            .Then(_ => IsNotFound(), "Entonces la publicación no se encuentra")
            .BDDfy("No se puede restablecer una publicación que no existe");

    private void AListing() => Exists(_listing = ListingFactory.InStatus(ListingStatus.Suspended));

    private void AMissingListing() => _listing = ListingFactory.InStatus(ListingStatus.Suspended);

    private Task Reinstates()
        => TryAsync(() => Handler<ReinstateListingHandler>()
            .HandleAsync(new ReinstateListingCommand(_listing.Id), User, CancellationToken.None));

    private void StatusIs(ListingStatus expected) => Assert.AreEqual(expected, _listing.Status);
}
