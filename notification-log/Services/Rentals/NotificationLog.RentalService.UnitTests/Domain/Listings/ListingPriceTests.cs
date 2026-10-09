using NotificationLog.RentalService.Domain.Common.ValueObjects;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.UnitTests.Domain.Listings;

[TestClass]
[TestCategory("Domain-Listings")]
public class ListingPriceTests : DomainScenario
{
    private Listing _listing = null!;

    private Operation Operation { get; set; }
    private long Price { get; set; }
    private bool Allowed { get; set; }
    private ListingStatus Status { get; set; }
    private string Moment { get; set; } = string.Empty;
    private string Result { get; set; } = string.Empty;

    [TestMethod]
    public void ThePriceHasAMinimumPerOperation() =>
        this.Given(_ => ADraftFor(Operation), "Dado un borrador de operación <operation>")
            .When(_ => PriceIsSetTo(Price), "Cuando el propietario fija el precio en <price>")
            .Then(_ => IsAccepted(Allowed), "Entonces el precio se acepta: <allowed>")
            .WithExamples(new ExampleTable("operation", "price", "allowed")
            {
                { Operation.Rent, 299_999L, false },
                { Operation.Rent, 300_000L, true },
                { Operation.Sale, 29_999_999L, false },
                { Operation.Sale, 30_000_000L, true },
            })
            .BDDfy("El precio tiene un mínimo según la operación");

    [TestMethod]
    public void APublishedListingChangesPriceOnceEvery24Hours() =>
        this.Given(_ => APublishedListingWhosePriceChangedOnOctober5At9(),
                "Dada una publicación publicada cuyo precio cambió el 2026-10-05 09:00")
            .And(_ => ItIs(Moment), "Y son las <moment>")
            .When(_ => PriceIsSetTo(2_000_000L), "Cuando el propietario fija el precio en 2000000")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("moment", "result")
            {
                { "2026-10-05 09:01", "El precio solo se puede cambiar una vez cada 24 horas." },
                { "2026-10-06 08:59", "El precio solo se puede cambiar una vez cada 24 horas." },
                { "2026-10-06 09:00", Accepted },
            })
            .BDDfy("El precio de una publicación publicada cambia una vez cada 24 horas");

    [TestMethod]
    public void ADraftChangesPriceWithoutWaiting() =>
        this.Given(_ => ADraftFor(Operation.Rent), "Dado un borrador de arriendo")
            .When(_ => PriceIsSetTo(1_800_000L), "Cuando el propietario fija el precio en 1800000")
            .And(_ => PriceIsSetTo(1_900_000L), "Y enseguida lo fija en 1900000")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => PriceIs(1_900_000L), "Y el precio queda en 1900000")
            .BDDfy("Un borrador cambia de precio sin esperar");

    [TestMethod]
    public void SettingTheSamePriceHasNoEffect() =>
        this.Given(_ => AListingInStatus(ListingStatus.Published), "Dada una publicación publicada con precio 1800000")
            .When(_ => PriceIsSetTo(1_800_000L), "Cuando el propietario fija el precio en 1800000")
            .Then(_ => NothingIsRecorded(), "Entonces no se registra ningún cambio")
            .BDDfy("Fijar el mismo precio no tiene efecto");

    [TestMethod]
    public void ThePriceCannotChangeOnceClosedWithdrawnOrSuspended() =>
        this.Given(_ => AListingInStatus(Status), "Dada una publicación en estado <status>")
            .When(_ => PriceIsSetTo(2_000_000L), "Cuando el propietario fija el precio en 2000000")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("status", "result")
            {
                { ListingStatus.InReview, Accepted },
                { ListingStatus.Paused, Accepted },
                { ListingStatus.Expired, Accepted },
                { ListingStatus.Closed, "El precio de esta publicación ya no se puede cambiar." },
                { ListingStatus.Withdrawn, "El precio de esta publicación ya no se puede cambiar." },
                { ListingStatus.Suspended, "El precio de esta publicación ya no se puede cambiar." },
            })
            .BDDfy("El precio no cambia en una publicación cerrada, retirada o suspendida");

    private void ADraftFor(Operation operation) => _listing = ListingFactory.Draft(operation);

    private void AListingInStatus(ListingStatus status) => _listing = ListingFactory.InStatus(status);

    private void APublishedListingWhosePriceChangedOnOctober5At9()
    {
        _listing = ListingFactory.InStatus(ListingStatus.Published);
        _listing.ChangePrice(Money.Create(1_950_000), Clock.Now);
        _listing.ClearDomainEvents();
    }

    private void PriceIsSetTo(long price) => Try(() => _listing.ChangePrice(Money.Create(price), Now));

    private void PriceIs(long expected) => Assert.AreEqual(expected, _listing.Price?.Amount);

    private void NothingIsRecorded() => HasNoEffect(_listing);
}
