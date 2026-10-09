using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.UnitTests.Domain.Listings;

[TestClass]
[TestCategory("Domain-Listings")]
public class ListingWithdrawalTests : DomainScenario
{
    private Listing _listing = null!;

    private ListingStatus Status { get; set; }
    private int Length { get; set; }
    private string Result { get; set; } = string.Empty;

    [TestMethod]
    public void WithdrawingAListingLeavesItWithdrawn() =>
        this.Given(_ => AListingInStatus(ListingStatus.Published), "Dada una publicación publicada")
            .When(_ => IsWithdrawn(), "Cuando el propietario la retira")
            .Then(_ => StatusIs(ListingStatus.Withdrawn), "Entonces la publicación queda retirada")
            .BDDfy("Retirar una publicación la deja retirada");

    [TestMethod]
    public void AnyListingThatIsNotClosedCanBeWithdrawn() =>
        this.Given(_ => AListingInStatus(Status), "Dada una publicación en estado <status>")
            .When(_ => IsWithdrawn(), "Cuando el propietario la retira")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("status", "result")
            {
                { ListingStatus.Draft, Accepted },
                { ListingStatus.InReview, Accepted },
                { ListingStatus.Published, Accepted },
                { ListingStatus.Paused, Accepted },
                { ListingStatus.Expired, Accepted },
                { ListingStatus.Suspended, Accepted },
                { ListingStatus.Withdrawn, Accepted },
                { ListingStatus.Closed, "Una publicación cerrada no se puede retirar." },
            })
            .BDDfy("Se retira cualquier publicación que no esté cerrada");

    [TestMethod]
    public void WithdrawingAWithdrawnListingHasNoEffect() =>
        this.Given(_ => AListingInStatus(ListingStatus.Withdrawn), "Dada una publicación retirada")
            .When(_ => IsWithdrawn(), "Cuando el propietario la retira de nuevo")
            .Then(_ => NothingIsRecorded(), "Entonces no se registra ningún cambio")
            .BDDfy("Retirar una publicación ya retirada no tiene efecto");

    [TestMethod]
    public void AReasonOfUpTo500CharactersIsRequired() =>
        this.Given(_ => AListingInStatus(ListingStatus.Published), "Dada una publicación publicada")
            .When(_ => IsWithdrawnWithReasonOfLength(Length), "Cuando el propietario la retira con un motivo de <length> caracteres")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("length", "result")
            {
                { 0, "Hay que indicar el motivo." },
                { 1, Accepted },
                { 500, Accepted },
                { 501, "El motivo no puede superar 500 caracteres." },
            })
            .BDDfy("Hay que indicar un motivo de hasta 500 caracteres");

    private void AListingInStatus(ListingStatus status) => _listing = ListingFactory.InStatus(status);

    private void IsWithdrawn() => Try(() => _listing.Withdraw("Decidí no arrendar"));

    private void IsWithdrawnWithReasonOfLength(int length) => Try(() => _listing.Withdraw(new string('x', length)));

    private void StatusIs(ListingStatus expected) => Assert.AreEqual(expected, _listing.Status);

    private void NothingIsRecorded() => HasNoEffect(_listing);
}
