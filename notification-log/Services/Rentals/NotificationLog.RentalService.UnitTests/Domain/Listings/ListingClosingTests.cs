using System.Globalization;
using NotificationLog.RentalService.Domain.Common.ValueObjects;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.UnitTests.Domain.Listings;

[TestClass]
public class ListingClosingTests : DomainScenario
{
    private Listing _listing = null!;

    private ListingStatus Status { get; set; }
    private long Amount { get; set; }
    private string Signed { get; set; } = string.Empty;
    private string Result { get; set; } = string.Empty;

    [TestMethod]
    public void ClosingAPublishedListingLeavesItClosed() =>
        this.Given(_ => AListingInStatus(ListingStatus.Published), "Dada una publicación publicada")
            .When(_ => IsClosedFor(1_750_000L, "2026-10-04"), "Cuando el propietario la cierra por 1750000, firmada el 2026-10-04")
            .Then(_ => StatusIs(ListingStatus.Closed), "Entonces la publicación queda cerrada")
            .BDDfy("Cerrar una publicación publicada la deja cerrada");

    [TestMethod]
    public void OnlyAVisibleListingCanBeClosed() =>
        this.Given(_ => AListingInStatus(Status), "Dada una publicación en estado <status>")
            .When(_ => IsClosedFor(1_750_000L, "2026-10-04"), "Cuando el propietario la cierra")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("status", "result")
            {
                { ListingStatus.Published, Accepted },
                { ListingStatus.Paused, Accepted },
                { ListingStatus.Draft, "Solo se puede cerrar una publicación publicada o pausada." },
                { ListingStatus.InReview, "Solo se puede cerrar una publicación publicada o pausada." },
                { ListingStatus.Expired, "Solo se puede cerrar una publicación publicada o pausada." },
                { ListingStatus.Suspended, "Solo se puede cerrar una publicación publicada o pausada." },
                { ListingStatus.Closed, "Solo se puede cerrar una publicación publicada o pausada." },
            })
            .BDDfy("Solo se cierra una publicación visible");

    [TestMethod]
    public void ClosingNeedsAPositiveAmountAndANonFutureSignature() =>
        this.Given(_ => AListingInStatus(ListingStatus.Published), "Dada una publicación publicada, hoy 2026-10-05")
            .When(_ => IsClosedFor(Amount, Signed), "Cuando el propietario la cierra por <amount>, firmada el <signed>")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("amount", "signed", "result")
            {
                { 1L, "2026-10-05", Accepted },
                { 0L, "2026-10-05", "El valor final debe ser mayor que cero." },
                { 1_750_000L, "2026-10-06", "La fecha de firma no puede estar en el futuro." },
            })
            .BDDfy("El cierre necesita un valor mayor que cero y una firma que no sea futura");

    private void AListingInStatus(ListingStatus status) => _listing = ListingFactory.InStatus(status);

    private void IsClosedFor(long finalPrice, string signedOn)
        => Try(() => _listing.Close(
            Money.Create(finalPrice), DateOnly.ParseExact(signedOn, "yyyy-MM-dd", CultureInfo.InvariantCulture), Now));

    private void StatusIs(ListingStatus expected) => Assert.AreEqual(expected, _listing.Status);
}
