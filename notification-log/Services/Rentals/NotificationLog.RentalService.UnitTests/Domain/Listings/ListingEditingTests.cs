using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.UnitTests.Domain.Listings;

[TestClass]
public class ListingEditingTests : DomainScenario
{
    private const string NewDetails = "cambiar los datos";
    private const string NewPhoto = "subir una foto";
    private const string RemovePhoto = "quitar una foto";
    private const string ReorderPhotos = "reordenar las fotos";

    private Listing _listing = null!;

    private ListingStatus Status { get; set; }
    private string Change { get; set; } = string.Empty;
    private ListingStatus Becomes { get; set; }

    [TestMethod]
    public void EditingAVisibleListingSendsItBackToReview() =>
        this.Given(_ => AListingInStatus(Status), "Dada una publicación en estado <status>")
            .When(_ => IsEditedBy(Change), "Cuando el propietario decide <change>")
            .Then(_ => StatusIs(Becomes), "Entonces la publicación queda en estado <becomes>")
            .WithExamples(new ExampleTable("status", "change", "becomes")
            {
                { ListingStatus.Draft, NewDetails, ListingStatus.Draft },
                { ListingStatus.Draft, NewPhoto, ListingStatus.Draft },
                { ListingStatus.InReview, NewDetails, ListingStatus.InReview },
                { ListingStatus.InReview, NewPhoto, ListingStatus.InReview },
                { ListingStatus.Published, NewDetails, ListingStatus.InReview },
                { ListingStatus.Published, NewPhoto, ListingStatus.InReview },
                { ListingStatus.Published, RemovePhoto, ListingStatus.InReview },
                { ListingStatus.Published, ReorderPhotos, ListingStatus.InReview },
                { ListingStatus.Paused, NewDetails, ListingStatus.InReview },
                { ListingStatus.Paused, NewPhoto, ListingStatus.InReview },
            })
            .BDDfy("Editar una publicación visible la devuelve a revisión");

    [TestMethod]
    public void AListingOutOfCirculationCannotBeEdited() =>
        this.Given(_ => AListingInStatus(Status), "Dada una publicación en estado <status>")
            .When(_ => IsEditedBy(Change), "Cuando el propietario decide <change>")
            .Then(_ => IsRejectedWith("Solo se puede editar una publicación en borrador, en revisión, publicada o pausada."),
                "Entonces se rechaza: Solo se puede editar una publicación en borrador, en revisión, publicada o pausada.")
            .WithExamples(new ExampleTable("status", "change")
            {
                { ListingStatus.Expired, NewDetails },
                { ListingStatus.Closed, NewDetails },
                { ListingStatus.Withdrawn, NewDetails },
                { ListingStatus.Suspended, NewDetails },
                { ListingStatus.Suspended, NewPhoto },
                { ListingStatus.Suspended, RemovePhoto },
                { ListingStatus.Suspended, ReorderPhotos },
            })
            .BDDfy("No se edita una publicación fuera de circulación");

    [TestMethod]
    public void SavingTheSameDetailsHasNoEffect() =>
        this.Given(_ => AListingInStatus(ListingStatus.Published), "Dada una publicación publicada")
            .When(_ => TheSameDetailsAreSaved(), "Cuando el propietario guarda los mismos datos")
            .Then(_ => StatusIs(ListingStatus.Published), "Entonces la publicación sigue publicada")
            .And(_ => NothingIsRecorded(), "Y no se registra ningún cambio")
            .BDDfy("Guardar los mismos datos no tiene efecto");

    private void AListingInStatus(ListingStatus status) => _listing = ListingFactory.InStatus(status);

    private void IsEditedBy(string change) => Try(() =>
    {
        switch (change)
        {
            case NewDetails:
                _listing.UpdateDetails(
                    ListingFactory.SampleDetails(), ListingFactory.SampleLocation("Usaquén"), ListingFactory.SampleDescription());
                break;
            case NewPhoto:
                _listing.AddPhoto(ListingFactory.NewPhoto());
                break;
            case RemovePhoto:
                _listing.RemovePhoto(_listing.Photos[0].FileName);
                break;
            case ReorderPhotos:
                _listing.ReorderPhotos(_listing.Photos.Select(photo => photo.FileName).Reverse().ToList());
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(change), change, null);
        }
    });

    private void TheSameDetailsAreSaved()
        => _listing.UpdateDetails(
            ListingFactory.SampleDetails(), ListingFactory.SampleLocation(), ListingFactory.SampleDescription());

    private void StatusIs(ListingStatus expected) => Assert.AreEqual(expected, _listing.Status);

    private void NothingIsRecorded() => HasNoEffect(_listing);
}
