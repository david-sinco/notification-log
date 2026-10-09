using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;
using NotificationLog.RentalService.Domain.Listings.Events;

namespace NotificationLog.RentalService.UnitTests.Domain.Listings;

[TestClass]
[TestCategory("Domain-Listings")]
public class ListingModerationTests : DomainScenario
{
    private Listing _listing = null!;

    private ListingStatus Status { get; set; }
    private ListingStatus Becomes { get; set; }
    private string Moment { get; set; } = string.Empty;
    private string Result { get; set; } = string.Empty;

    [TestMethod]
    public void ApprovingPublishesTheListingForOneMonth() =>
        this.Given(_ => AListingInStatus(ListingStatus.InReview), "Dada una publicación en revisión el 2026-10-05 09:00")
            .When(_ => IsApproved(), "Cuando el moderador la aprueba")
            .Then(_ => StatusIs(ListingStatus.Published), "Entonces la publicación queda publicada")
            .And(_ => ExpiresOn("2026-11-05 09:00"), "Y vence el 2026-11-05 09:00")
            .BDDfy("Aprobar publica la publicación con un mes de vigencia");

    [TestMethod]
    public void OnlyAListingInReviewCanBeApproved() =>
        this.Given(_ => AListingInStatus(Status), "Dada una publicación en estado <status>")
            .When(_ => IsApproved(), "Cuando el moderador la aprueba")
            .Then(_ => IsRejectedWith("Solo se puede aprobar una publicación en revisión."),
                "Entonces se rechaza: Solo se puede aprobar una publicación en revisión.")
            .WithExamples(new ExampleTable("status")
            {
                { ListingStatus.Draft },
                { ListingStatus.Published },
                { ListingStatus.Paused },
                { ListingStatus.Suspended },
                { ListingStatus.Withdrawn },
            })
            .BDDfy("Solo se aprueba una publicación en revisión");

    [TestMethod]
    public void RejectingSendsTheListingBackToDraftWithItsReasons() =>
        this.Given(_ => AListingInStatus(ListingStatus.InReview), "Dada una publicación en revisión")
            .When(_ => IsRejectedForLowQualityPhotosAndInvalidAddressRepeatingOne(),
                "Cuando el moderador la rechaza por fotos de baja calidad y dirección inválida, repitiendo un motivo")
            .Then(_ => StatusIs(ListingStatus.Draft), "Entonces la publicación vuelve a borrador")
            .And(_ => RejectionReasonsAreRecorded(2), "Y se registran 2 motivos de rechazo")
            .BDDfy("Rechazar devuelve la publicación a borrador con sus motivos");

    [TestMethod]
    public void RejectingNeedsAtLeastOneReason() =>
        this.Given(_ => AListingInStatus(ListingStatus.InReview), "Dada una publicación en revisión")
            .When(_ => IsRejectedWithoutReasons(), "Cuando el moderador la rechaza sin motivos")
            .Then(_ => IsRejectedWith("Hay que indicar al menos un motivo de rechazo."),
                "Entonces se rechaza: Hay que indicar al menos un motivo de rechazo.")
            .BDDfy("Rechazar exige al menos un motivo");

    [TestMethod]
    public void OnlyAListingInReviewCanBeRejected() =>
        this.Given(_ => AListingInStatus(Status), "Dada una publicación en estado <status>")
            .When(_ => IsRejectedForLowQualityPhotosAndInvalidAddressRepeatingOne(), "Cuando el moderador la rechaza")
            .Then(_ => IsRejectedWith("Solo se puede rechazar una publicación en revisión."),
                "Entonces se rechaza: Solo se puede rechazar una publicación en revisión.")
            .WithExamples(new ExampleTable("status")
            {
                { ListingStatus.Draft },
                { ListingStatus.Published },
                { ListingStatus.Suspended },
            })
            .BDDfy("Solo se rechaza una publicación en revisión");

    [TestMethod]
    public void OnlyAVisibleListingCanBeSuspended() =>
        this.Given(_ => AListingInStatus(Status), "Dada una publicación en estado <status>")
            .When(_ => IsSuspended(), "Cuando el moderador la suspende")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("status", "result")
            {
                { ListingStatus.Published, Accepted },
                { ListingStatus.Paused, Accepted },
                { ListingStatus.Suspended, Accepted },
                { ListingStatus.Draft, "Solo se puede suspender una publicación publicada o pausada." },
                { ListingStatus.InReview, "Solo se puede suspender una publicación publicada o pausada." },
                { ListingStatus.Expired, "Solo se puede suspender una publicación publicada o pausada." },
                { ListingStatus.Closed, "Solo se puede suspender una publicación publicada o pausada." },
            })
            .BDDfy("Solo se suspende una publicación visible");

    [TestMethod]
    public void SuspendingLeavesTheListingSuspended() =>
        this.Given(_ => AListingInStatus(ListingStatus.Published), "Dada una publicación publicada")
            .When(_ => IsSuspended(), "Cuando el moderador la suspende")
            .Then(_ => StatusIs(ListingStatus.Suspended), "Entonces la publicación queda suspendida")
            .BDDfy("Suspender deja la publicación suspendida");

    [TestMethod]
    public void ReinstatingPublishesAgainUnlessTheValidityHasPassed() =>
        this.Given(_ => AListingInStatus(ListingStatus.Suspended), "Dada una publicación suspendida que vence el 2026-11-05 09:00")
            .And(_ => ItIs(Moment), "Y son las <moment>")
            .When(_ => IsReinstated(), "Cuando el moderador la restablece")
            .Then(_ => StatusIs(Becomes), "Entonces la publicación queda en estado <becomes>")
            .WithExamples(new ExampleTable("moment", "becomes")
            {
                { "2026-10-20 09:00", ListingStatus.Published },
                { "2026-11-05 08:59", ListingStatus.Published },
                { "2026-11-05 09:00", ListingStatus.Expired },
                { "2026-11-14 09:00", ListingStatus.Expired },
            })
            .BDDfy("Restablecer vuelve a publicar, salvo que la vigencia haya vencido");

    [TestMethod]
    public void OnlyASuspendedListingCanBeReinstated() =>
        this.Given(_ => AListingInStatus(ListingStatus.Published), "Dada una publicación publicada")
            .When(_ => IsReinstated(), "Cuando el moderador la restablece")
            .Then(_ => IsRejectedWith("La publicación no está suspendida."), "Entonces se rechaza: La publicación no está suspendida.")
            .BDDfy("Solo se restablece una publicación suspendida");

    private void AListingInStatus(ListingStatus status) => _listing = ListingFactory.InStatus(status);

    private void IsApproved() => Try(() => _listing.Approve(ListingFactory.Moderator, Now));

    private void IsRejectedForLowQualityPhotosAndInvalidAddressRepeatingOne()
        => Try(() => _listing.Reject(
            ListingFactory.Moderator,
            [RejectionReason.LowQualityPhotos, RejectionReason.InvalidAddress, RejectionReason.LowQualityPhotos]));

    private void IsRejectedWithoutReasons() => Try(() => _listing.Reject(ListingFactory.Moderator, []));

    private void IsSuspended() => Try(() => _listing.Suspend("Denuncia por datos falsos"));

    private void IsReinstated() => Try(() => _listing.Reinstate(ListingFactory.Moderator, Now));

    private void StatusIs(ListingStatus expected) => Assert.AreEqual(expected, _listing.Status);

    private void ExpiresOn(string moment) => Assert.AreEqual(Clock.At(moment), _listing.ExpiresAt);

    private void RejectionReasonsAreRecorded(int count)
        => Assert.HasCount(count, _listing.DomainEvents.OfType<ListingRejected>().Single().Reasons);
}
