using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.UnitTests.Domain.Listings;

[TestClass]
[TestCategory("Domain-Listings")]
public class ListingValidityTests : DomainScenario
{
    private const string OutOfRenewalWindow =
        "Solo se puede renovar una publicación vencida o a la que le queden 7 días o menos.";

    private Listing _listing = null!;

    private ListingStatus Status { get; set; }
    private ListingStatus Becomes { get; set; }
    private string Moment { get; set; } = string.Empty;
    private string Result { get; set; } = string.Empty;

    [TestMethod]
    public void APublishedListingIsRenewedWithin7DaysOfExpiry() =>
        this.Given(_ => APublishedListingExpiringOnNovember5At9(), "Dada una publicación publicada que vence el 2026-11-05 09:00")
            .And(_ => ItIs(Moment), "Y son las <moment>")
            .When(_ => IsRenewed(), "Cuando el propietario la renueva")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("moment", "result")
            {
                { "2026-10-15 09:00", OutOfRenewalWindow },
                { "2026-10-29 08:59", OutOfRenewalWindow },
                { "2026-10-29 09:00", Accepted },
                { "2026-11-05 08:59", Accepted },
            })
            .BDDfy("Una publicación publicada se renueva cuando le quedan 7 días o menos");

    [TestMethod]
    public void RenewingExtendsTheValidityOneMonthFromTheRenewal() =>
        this.Given(_ => APublishedListingExpiringOnNovember5At9(), "Dada una publicación publicada que vence el 2026-11-05 09:00")
            .And(_ => ItIs("2026-10-30 09:00"), "Y son las 2026-10-30 09:00")
            .When(_ => IsRenewed(), "Cuando el propietario la renueva")
            .Then(_ => ExpiresOn("2026-11-30 09:00"), "Entonces la publicación vence el 2026-11-30 09:00")
            .BDDfy("Renovar extiende la vigencia un mes desde el momento de la renovación");

    [TestMethod]
    public void AnExpiredListingIsPublishedAgainWhenRenewed() =>
        this.Given(_ => AListingInStatus(ListingStatus.Expired), "Dada una publicación vencida")
            .And(_ => ItIs("2026-11-10 09:00"), "Y son las 2026-11-10 09:00")
            .When(_ => IsRenewed(), "Cuando el propietario la renueva")
            .Then(_ => StatusIs(ListingStatus.Published), "Entonces la publicación vuelve a estar publicada")
            .And(_ => ExpiresOn("2026-12-10 09:00"), "Y vence el 2026-12-10 09:00")
            .BDDfy("Una publicación vencida vuelve a publicarse al renovarla");

    [TestMethod]
    public void OnlyAPublishedOrExpiredListingCanBeRenewed() =>
        this.Given(_ => AListingInStatus(Status), "Dada una publicación en estado <status>")
            .And(_ => ItIs("2026-11-01 09:00"), "Y son las 2026-11-01 09:00")
            .When(_ => IsRenewed(), "Cuando el propietario la renueva")
            .Then(_ => IsRejectedWith(OutOfRenewalWindow),
                "Entonces se rechaza: Solo se puede renovar una publicación vencida o a la que le queden 7 días o menos.")
            .WithExamples(new ExampleTable("status")
            {
                { ListingStatus.Draft },
                { ListingStatus.InReview },
                { ListingStatus.Paused },
                { ListingStatus.Suspended },
                { ListingStatus.Closed },
                { ListingStatus.Withdrawn },
            })
            .BDDfy("Solo se renueva una publicación publicada o vencida");

    [TestMethod]
    public void AVisibleListingExpiresWhenItsValidityEnds() =>
        this.Given(_ => AListingInStatus(Status), "Dada una publicación en estado <status>, con vigencia hasta el 2026-11-05 09:00")
            .And(_ => ItIs(Moment), "Y son las <moment>")
            .When(_ => SystemChecksTheValidity(), "Cuando el sistema revisa la vigencia")
            .Then(_ => StatusIs(Becomes), "Entonces la publicación queda en estado <becomes>")
            .WithExamples(new ExampleTable("status", "moment", "becomes")
            {
                { ListingStatus.Published, "2026-11-05 08:59", ListingStatus.Published },
                { ListingStatus.Published, "2026-11-05 09:00", ListingStatus.Expired },
                { ListingStatus.Paused, "2026-11-05 08:59", ListingStatus.Paused },
                { ListingStatus.Paused, "2026-11-05 09:00", ListingStatus.Expired },
                { ListingStatus.Suspended, "2026-11-05 09:00", ListingStatus.Suspended },
                { ListingStatus.Draft, "2026-11-05 09:00", ListingStatus.Draft },
                { ListingStatus.InReview, "2026-11-05 09:00", ListingStatus.InReview },
            })
            .BDDfy("Una publicación visible vence al cumplirse su vigencia");

    private void APublishedListingExpiringOnNovember5At9() => _listing = ListingFactory.InStatus(ListingStatus.Published);

    private void AListingInStatus(ListingStatus status) => _listing = ListingFactory.InStatus(status);

    private void IsRenewed() => Try(() => _listing.Renew(Now));

    private void SystemChecksTheValidity() => _listing.Expire(Now);

    private void StatusIs(ListingStatus expected) => Assert.AreEqual(expected, _listing.Status);

    private void ExpiresOn(string moment) => Assert.AreEqual(Clock.At(moment), _listing.ExpiresAt);
}
