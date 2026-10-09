using NotificationLog.RentalService.Application.Listings.Commands.ExpireListing;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.UnitTests.Application.Listings.Commands.ExpireListing;

[TestClass]
[TestCategory("Application-Listings")]
public class ExpireListingHandlerTests : ApplicationScenario
{
    private Listing _listing = null!;

    private string Moment { get; set; } = string.Empty;
    private ListingStatus Becomes { get; set; }

    [TestMethod]
    public void AListingExpiresWhenItsValidityHasPassed() =>
        this.Given(_ => AListing(), "Dada una publicación publicada que vence el 2026-11-05 09:00")
            .And(_ => ItIs(Moment), "Y son las <moment>")
            .When(_ => Expires(), "Cuando el sistema procesa su vencimiento")
            .Then(_ => StatusIs(Becomes), "Entonces la publicación queda en estado <becomes>")
            .WithExamples(new ExampleTable("moment", "becomes")
            {
                { "2026-11-05 08:59", ListingStatus.Published },
                { "2026-11-05 09:00", ListingStatus.Expired },
            })
            .BDDfy("Una publicación vence cuando su vigencia ya pasó según la hora actual");

    [TestMethod]
    public void AnExpiredListingIsSaved() =>
        this.Given(_ => AListing(), "Dada una publicación publicada que vence el 2026-11-05 09:00")
            .And(_ => ItIs("2026-11-06 09:00"), "Y son las 2026-11-06 09:00")
            .When(_ => Expires(), "Cuando el sistema procesa su vencimiento")
            .Then(_ => IsSaved(_listing), "Entonces se guarda la publicación vencida")
            .BDDfy("El vencimiento se guarda");

    [TestMethod]
    public void AMissingListingIsIgnored() =>
        this.Given(_ => AMissingListing(), "Dada una publicación que no existe")
            .When(_ => Expires(), "Cuando el sistema procesa su vencimiento")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta sin error")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .BDDfy("El vencimiento de una publicación que no existe se ignora");

    private void AListing() => Exists(_listing = ListingFactory.InStatus(ListingStatus.Published));

    private void AMissingListing() => _listing = ListingFactory.InStatus(ListingStatus.Published);

    private Task Expires()
        => TryAsync(() => Handler<ExpireListingHandler>().HandleAsync(new ExpireListingCommand(_listing.Id), CancellationToken.None));

    private void StatusIs(ListingStatus expected) => Assert.AreEqual(expected, _listing.Status);
}
