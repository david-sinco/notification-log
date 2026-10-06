using NotificationLog.RentalService.Application.Listings.Commands.ReorderListingPhotos;
using NotificationLog.RentalService.Domain.Listings;

namespace NotificationLog.RentalService.UnitTests.Application.Listings.Commands.ReorderListingPhotos;

[TestClass]
public class ReorderListingPhotosHandlerTests : ApplicationScenario
{
    private Listing _listing = null!;
    private List<string> _reversed = [];

    [TestMethod]
    public void TheOwnerReordersThePhotosOfTheirListing() =>
        this.Given(_ => AListing(), "Dada una publicación con 5 fotos")
            .And(_ => OwnerSignsIn(), "Y que su propietario inicia sesión")
            .When(_ => ReordersWith(_reversed), "Cuando envía las fotos en orden inverso")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => PhotosAreInTheNewOrder(), "Y las fotos quedan en el nuevo orden")
            .And(_ => IsSaved(_listing), "Y se guarda el cambio")
            .BDDfy("El propietario reordena las fotos de su publicación");

    [TestMethod]
    public void TheOrderIsRequired() =>
        this.Given(_ => AListing(), "Dada una publicación con 5 fotos")
            .And(_ => OwnerSignsIn(), "Y que su propietario inicia sesión")
            .When(_ => ReordersWith(null!), "Cuando no envía ningún orden")
            .Then(_ => FailsValidationOn("FileNames"), "Entonces la validación falla en la lista de fotos")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .BDDfy("Reordenar exige la lista de fotos");

    [TestMethod]
    public void AnotherOwnerCannotReorderThePhotos() =>
        this.Given(_ => AListing(), "Dada una publicación con 5 fotos")
            .And(_ => AnotherOwnerSignsIn(), "Y que otro propietario inicia sesión")
            .When(_ => ReordersWith(_reversed), "Cuando envía las fotos en orden inverso")
            .Then(_ => IsForbidden(), "Entonces se rechaza por falta de permiso")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .BDDfy("Otro propietario no puede reordenar las fotos de la publicación");

    [TestMethod]
    public void AMissingListingIsNotFound() =>
        this.Given(_ => AMissingListing(), "Dada una publicación que no existe")
            .And(_ => OwnerSignsIn(), "Y que un propietario inicia sesión")
            .When(_ => ReordersWith(_reversed), "Cuando envía un orden de fotos")
            .Then(_ => IsNotFound(), "Entonces la publicación no se encuentra")
            .BDDfy("No se pueden reordenar las fotos de una publicación que no existe");

    private void AListing()
    {
        AMissingListing();
        Exists(_listing);
    }

    private void AMissingListing()
    {
        _listing = ListingFactory.Complete();
        _reversed = _listing.Photos.Select(photo => photo.FileName).Reverse().ToList();
    }

    private Task ReordersWith(IReadOnlyList<string> fileNames)
        => TryAsync(() => Handler<ReorderListingPhotosHandler>()
            .HandleAsync(new ReorderListingPhotosCommand(_listing.Id, fileNames), User, CancellationToken.None));

    private void PhotosAreInTheNewOrder()
        => CollectionAssert.AreEqual(_reversed, _listing.Photos.Select(photo => photo.FileName).ToList());
}
