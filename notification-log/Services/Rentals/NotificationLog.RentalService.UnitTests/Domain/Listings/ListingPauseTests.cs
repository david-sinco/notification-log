using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.UnitTests.Domain.Listings;

[TestClass]
public class ListingPauseTests : DomainScenario
{
    private Listing _listing = null!;

    private ListingStatus Status { get; set; }
    private string Result { get; set; } = string.Empty;

    [TestMethod]
    public void APublishedListingIsPausedAndResumed() =>
        this.Given(_ => AListingInStatus(ListingStatus.Published), "Dada una publicación publicada")
            .When(_ => IsPaused(), "Cuando el propietario la pausa")
            .Then(_ => StatusIs(ListingStatus.Paused), "Entonces la publicación queda pausada")
            .When(_ => IsResumed(), "Cuando el propietario la reanuda")
            .Then(_ => StatusIs(ListingStatus.Published), "Entonces la publicación vuelve a estar publicada")
            .BDDfy("Una publicación publicada se pausa y se reanuda");

    [TestMethod]
    public void OnlyAPublishedListingCanBePaused() =>
        this.Given(_ => AListingInStatus(Status), "Dada una publicación en estado <status>")
            .When(_ => IsPaused(), "Cuando el propietario la pausa")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("status", "result")
            {
                { ListingStatus.Published, Accepted },
                { ListingStatus.Paused, Accepted },
                { ListingStatus.Draft, "Solo se puede pausar una publicación publicada." },
                { ListingStatus.InReview, "Solo se puede pausar una publicación publicada." },
                { ListingStatus.Expired, "Solo se puede pausar una publicación publicada." },
                { ListingStatus.Suspended, "Solo se puede pausar una publicación publicada." },
            })
            .BDDfy("Solo se pausa una publicación publicada");

    [TestMethod]
    public void OnlyAPausedListingCanBeResumed() =>
        this.Given(_ => AListingInStatus(Status), "Dada una publicación en estado <status>")
            .When(_ => IsResumed(), "Cuando el propietario la reanuda")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("status", "result")
            {
                { ListingStatus.Paused, Accepted },
                { ListingStatus.Published, Accepted },
                { ListingStatus.Draft, "Solo se puede reanudar una publicación pausada." },
                { ListingStatus.Expired, "Solo se puede reanudar una publicación pausada." },
                { ListingStatus.Suspended, "Solo se puede reanudar una publicación pausada." },
            })
            .BDDfy("Solo se reanuda una publicación pausada");

    [TestMethod]
    public void PausingAPausedListingHasNoEffect() =>
        this.Given(_ => AListingInStatus(ListingStatus.Paused), "Dada una publicación pausada")
            .When(_ => IsPaused(), "Cuando el propietario la pausa de nuevo")
            .Then(_ => NothingIsRecorded(), "Entonces no se registra ningún cambio")
            .BDDfy("Pausar una publicación ya pausada no tiene efecto");

    [TestMethod]
    public void ResumingAPublishedListingHasNoEffect() =>
        this.Given(_ => AListingInStatus(ListingStatus.Published), "Dada una publicación publicada")
            .When(_ => IsResumed(), "Cuando el propietario la reanuda")
            .Then(_ => NothingIsRecorded(), "Entonces no se registra ningún cambio")
            .BDDfy("Reanudar una publicación ya publicada no tiene efecto");

    private void AListingInStatus(ListingStatus status) => _listing = ListingFactory.InStatus(status);

    private void IsPaused() => Try(_listing.Pause);

    private void IsResumed() => Try(_listing.Resume);

    private void StatusIs(ListingStatus expected) => Assert.AreEqual(expected, _listing.Status);

    private void NothingIsRecorded() => HasNoEffect(_listing);
}
