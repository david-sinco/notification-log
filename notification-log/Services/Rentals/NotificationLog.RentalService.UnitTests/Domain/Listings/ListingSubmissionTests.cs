using NotificationLog.RentalService.Domain.Common.ValueObjects;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.UnitTests.Domain.Listings;

[TestClass]
[TestCategory("Domain-Listings")]
public class ListingSubmissionTests : DomainScenario
{
    private Listing _listing = null!;

    private bool Details { get; set; }
    private bool Price { get; set; }
    private int Photos { get; set; }
    private ListingStatus Status { get; set; }
    private string Result { get; set; } = string.Empty;

    [TestMethod]
    public void ACompleteDraftGoesToReview() =>
        this.Given(_ => ADraftWith(true, true, 5), "Dado un borrador con datos del inmueble, precio y 5 fotos")
            .When(_ => IsSubmittedForReview(), "Cuando se envía a revisión")
            .Then(_ => StatusIs(ListingStatus.InReview), "Entonces la publicación queda en revisión")
            .BDDfy("Un borrador completo pasa a revisión");

    [TestMethod]
    public void SubmittingNeedsDetailsPriceAndAtLeast5Photos() =>
        this.Given(_ => ADraftWith(Details, Price, Photos),
                "Dado un borrador con datos del inmueble: <details>, con precio: <price> y con <photos> fotos")
            .When(_ => IsSubmittedForReview(), "Cuando se envía a revisión")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("details", "price", "photos", "result")
            {
                { true, true, 5, Accepted },
                { false, true, 5, "Faltan los datos del inmueble, la ubicación o la descripción." },
                { true, false, 5, "Falta el precio de la publicación." },
                { true, true, 4, "La publicación necesita al menos 5 fotos." },
                { true, true, 0, "La publicación necesita al menos 5 fotos." },
            })
            .BDDfy("Para enviar a revisión hacen falta datos, precio y al menos 5 fotos");

    [TestMethod]
    public void OnlyADraftCanBeSubmitted() =>
        this.Given(_ => AListingInStatus(Status), "Dada una publicación en estado <status>")
            .When(_ => IsSubmittedForReview(), "Cuando se envía a revisión")
            .Then(_ => IsRejectedWith("Solo se puede enviar a revisión una publicación en borrador."),
                "Entonces se rechaza: Solo se puede enviar a revisión una publicación en borrador.")
            .WithExamples(new ExampleTable("status")
            {
                { ListingStatus.InReview },
                { ListingStatus.Published },
                { ListingStatus.Paused },
                { ListingStatus.Expired },
                { ListingStatus.Closed },
                { ListingStatus.Withdrawn },
                { ListingStatus.Suspended },
            })
            .BDDfy("Solo se envía a revisión un borrador");

    private void ADraftWith(bool withDetails, bool withPrice, int photoCount)
    {
        _listing = ListingFactory.Draft();

        if (withDetails)
            _listing.UpdateDetails(
                ListingFactory.SampleDetails(), ListingFactory.SampleLocation(), ListingFactory.SampleDescription());

        if (withPrice)
            _listing.ChangePrice(Money.Create(1_800_000), Now);

        for (var i = 0; i < photoCount; i++)
            _listing.AddPhoto(ListingFactory.NewPhoto());
    }

    private void AListingInStatus(ListingStatus status) => _listing = ListingFactory.InStatus(status);

    private void IsSubmittedForReview() => Try(_listing.SubmitForReview);

    private void StatusIs(ListingStatus expected) => Assert.AreEqual(expected, _listing.Status);
}
