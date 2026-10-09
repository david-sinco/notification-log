using NotificationLog.RentalService.Application.Listings.Commands.AddListingPhoto;
using NotificationLog.RentalService.Domain.Listings;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace NotificationLog.RentalService.UnitTests.Application.Listings.Commands.AddListingPhoto;

[TestClass]
[TestCategory("Application-Listings")]
public class AddListingPhotoHandlerTests : ApplicationScenario
{
    private static readonly byte[] Jpeg = [0xFF, 0xD8, 0xFF, 0xE0];
    private static readonly byte[] NotAnImage = [0x25, 0x50, 0x44, 0x46];

    private Listing _listing = null!;
    private string _fileName = string.Empty;

    [TestMethod]
    public void TheOwnerAddsAPhotoToTheirListing() =>
        this.Given(_ => AListing(), "Dada una publicación en borrador sin fotos")
            .And(_ => OwnerSignsIn(), "Y que su propietario inicia sesión")
            .When(_ => Uploads(Jpeg), "Cuando sube una imagen JPEG")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => TheFileIsStoredAsJpeg(), "Y el archivo se guarda en el almacenamiento como image/jpeg con el nombre devuelto")
            .And(_ => IsSaved(_listing), "Y se guarda la publicación con la foto")
            .BDDfy("El propietario sube una foto a su publicación");

    [TestMethod]
    public void AFileThatIsNotAnImageIsNotStored() =>
        this.Given(_ => AListing(), "Dada una publicación en borrador sin fotos")
            .And(_ => OwnerSignsIn(), "Y que su propietario inicia sesión")
            .When(_ => Uploads(NotAnImage), "Cuando sube un archivo que no es una imagen")
            .Then(_ => IsRejectedWith("El archivo no es una imagen JPEG, PNG o WebP."),
                "Entonces se rechaza: El archivo no es una imagen JPEG, PNG o WebP.")
            .And(_ => NoFileIsStored(), "Y no se guarda ningún archivo")
            .BDDfy("Un archivo que no es imagen no llega al almacenamiento");

    [TestMethod]
    public void TheStoredFileIsDeletedWhenSavingFails() =>
        this.Given(_ => AListing(), "Dada una publicación en borrador sin fotos")
            .And(_ => OwnerSignsIn(), "Y que su propietario inicia sesión")
            .And(_ => SavingFails(), "Y que guardar la publicación falla")
            .When(_ => UploadFails(), "Cuando sube una imagen JPEG y el error se propaga")
            .Then(_ => TheStoredFileIsDeleted(), "Entonces el archivo ya guardado se borra del almacenamiento")
            .BDDfy("Si guardar la publicación falla, el archivo subido se borra");

    [TestMethod]
    public void AnotherOwnerCannotAddPhotos() =>
        this.Given(_ => AListing(), "Dada una publicación en borrador sin fotos")
            .And(_ => AnotherOwnerSignsIn(), "Y que otro propietario inicia sesión")
            .When(_ => Uploads(Jpeg), "Cuando sube una imagen JPEG")
            .Then(_ => IsForbidden(), "Entonces se rechaza por falta de permiso")
            .And(_ => NoFileIsStored(), "Y no se guarda ningún archivo")
            .BDDfy("Otro propietario no puede subir fotos a la publicación");

    [TestMethod]
    public void AMissingListingIsNotFound() =>
        this.Given(_ => AMissingListing(), "Dada una publicación que no existe")
            .And(_ => OwnerSignsIn(), "Y que un propietario inicia sesión")
            .When(_ => Uploads(Jpeg), "Cuando sube una imagen JPEG")
            .Then(_ => IsNotFound(), "Entonces la publicación no se encuentra")
            .BDDfy("No se puede subir una foto a una publicación que no existe");

    private void AListing() => Exists(_listing = ListingFactory.Draft());

    private void AMissingListing() => _listing = ListingFactory.Draft();

    private void SavingFails()
        => UnitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>()).ThrowsAsync(new InvalidOperationException("sin conexión"));

    private Task Uploads(byte[] content) => TryAsync(() => Upload(content));

    private Task UploadFails() => Assert.ThrowsExactlyAsync<InvalidOperationException>(() => Upload(Jpeg));

    private async Task Upload(byte[] content)
    {
        using var stream = new MemoryStream(content);

        _fileName = await Handler<AddListingPhotoHandler>()
            .HandleAsync(new AddListingPhotoCommand(_listing.Id, stream), User, CancellationToken.None);
    }

    private void TheFileIsStoredAsJpeg()
        => PhotoStorage.Received(1).SaveAsync(
            _listing.Id, _fileName, Arg.Any<Stream>(), "image/jpeg", Arg.Any<CancellationToken>());

    private void NoFileIsStored()
        => PhotoStorage.DidNotReceiveWithAnyArgs().SaveAsync(default, default!, default!, default!, default);

    private void TheStoredFileIsDeleted()
        => PhotoStorage.Received(1).DeleteAsync(_listing.Id, _listing.Photos[0].FileName, Arg.Any<CancellationToken>());
}
