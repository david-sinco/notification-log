using NotificationLog.RentalService.Domain.Listings;

namespace NotificationLog.RentalService.UnitTests.Domain.Listings;

[TestClass]
public class ListingPhotosTests : DomainScenario
{
    private Listing _listing = null!;
    private List<string> _initialOrder = [];

    private int Photos { get; set; }
    private string Result { get; set; } = string.Empty;

    [TestMethod]
    public void AListingTakesUpTo30Photos() =>
        this.Given(_ => ADraftWithPhotos(Photos), "Dado un borrador con <photos> fotos")
            .When(_ => ANewPhotoIsAdded(), "Cuando el propietario sube una foto nueva")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("photos", "result")
            {
                { 0, Accepted },
                { 29, Accepted },
                { 30, "Una publicación no puede tener más de 30 fotos." },
            })
            .BDDfy("Una publicación admite hasta 30 fotos");

    [TestMethod]
    public void TheSamePhotoCannotBeAddedTwice() =>
        this.Given(_ => ADraftWithPhotos(5), "Dado un borrador con 5 fotos")
            .When(_ => TheFirstPhotoIsAddedAgain(), "Cuando el propietario sube de nuevo la primera foto")
            .Then(_ => IsRejectedWith("Ya existe una foto con ese identificador en la publicación."),
                "Entonces se rechaza: Ya existe una foto con ese identificador en la publicación.")
            .BDDfy("No se sube dos veces la misma foto");

    [TestMethod]
    public void RemovingAPhotoTakesItOutOfTheListing() =>
        this.Given(_ => ADraftWithPhotos(5), "Dado un borrador con 5 fotos")
            .When(_ => TheFirstPhotoIsRemoved(), "Cuando el propietario quita la primera foto")
            .Then(_ => HasPhotos(4), "Entonces la publicación tiene 4 fotos")
            .And(_ => TheFirstPhotoIsGone(), "Y la primera foto ya no está")
            .BDDfy("Quitar una foto la saca de la publicación");

    [TestMethod]
    public void APhotoFromAnotherListingCannotBeRemoved() =>
        this.Given(_ => ADraftWithPhotos(5), "Dado un borrador con 5 fotos")
            .When(_ => AForeignPhotoIsRemoved(), "Cuando el propietario quita una foto que no es de la publicación")
            .Then(_ => IsRejectedWith("La foto no pertenece a esta publicación."),
                "Entonces se rechaza: La foto no pertenece a esta publicación.")
            .BDDfy("No se quita una foto que no es de la publicación");

    [TestMethod]
    public void ReorderingChangesThePhotoOrder() =>
        this.Given(_ => ADraftWithPhotos(5), "Dado un borrador con 5 fotos")
            .When(_ => TheOrderIsReversed(), "Cuando el propietario invierte el orden de las fotos")
            .Then(_ => PhotosAreInReverseOrder(), "Entonces las fotos quedan en orden inverso")
            .BDDfy("Reordenar cambia el orden de las fotos");

    [TestMethod]
    public void TheNewOrderMustIncludeExactlyTheListingPhotos() =>
        this.Given(_ => ADraftWithPhotos(5), "Dado un borrador con 5 fotos")
            .When(_ => TheOrderOmitsAPhoto(), "Cuando el propietario envía un orden al que le falta una foto")
            .Then(_ => IsRejectedWith("El nuevo orden debe incluir exactamente las fotos de la publicación."),
                "Entonces se rechaza: El nuevo orden debe incluir exactamente las fotos de la publicación.")
            .BDDfy("El nuevo orden debe incluir exactamente las fotos de la publicación");

    [TestMethod]
    public void ReorderingWithTheSameOrderHasNoEffect() =>
        this.Given(_ => ADraftWithPhotos(5), "Dado un borrador con 5 fotos")
            .When(_ => TheSameOrderIsSent(), "Cuando el propietario envía el mismo orden")
            .Then(_ => NothingIsRecorded(), "Entonces no se registra ningún cambio")
            .BDDfy("Reordenar con el mismo orden no tiene efecto");

    private void ADraftWithPhotos(int count)
    {
        _listing = ListingFactory.Complete(count);
        _initialOrder = Order();
    }

    private void ANewPhotoIsAdded() => Try(() => _listing.AddPhoto(ListingFactory.NewPhoto()));

    private void TheFirstPhotoIsAddedAgain() => Try(() => _listing.AddPhoto(_listing.Photos[0]));

    private void TheFirstPhotoIsRemoved() => Try(() => _listing.RemovePhoto(_initialOrder[0]));

    private void AForeignPhotoIsRemoved() => Try(() => _listing.RemovePhoto(ListingFactory.NewPhoto().FileName));

    private void TheOrderIsReversed() => Try(() => _listing.ReorderPhotos(Reversed()));

    private void TheOrderOmitsAPhoto() => Try(() => _listing.ReorderPhotos(_initialOrder.Skip(1).ToList()));

    private void TheSameOrderIsSent() => Try(() => _listing.ReorderPhotos(_initialOrder));

    private void HasPhotos(int count) => Assert.HasCount(count, _listing.Photos);

    private void TheFirstPhotoIsGone() => Assert.DoesNotContain(_initialOrder[0], Order());

    private void PhotosAreInReverseOrder() => CollectionAssert.AreEqual(Reversed(), Order());

    private void NothingIsRecorded() => HasNoEffect(_listing);

    private List<string> Order() => _listing.Photos.Select(photo => photo.FileName).ToList();

    private List<string> Reversed() => _initialOrder.AsEnumerable().Reverse().ToList();
}
