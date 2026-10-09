using NotificationLog.RentalService.Application.Listings.Commands.RemoveListingPhoto;
using NotificationLog.RentalService.Domain.Listings;
using NSubstitute;

namespace NotificationLog.RentalService.UnitTests.Application.Listings.Commands.RemoveListingPhoto;

[TestClass]
[TestCategory("Application-Listings")]
public class RemoveListingPhotoHandlerTests : ApplicationScenario
{
    private Listing _listing = null!;
    private string _fileName = string.Empty;

    [TestMethod]
    public void TheOwnerRemovesAPhotoFromTheirListing() =>
        this.Given(_ => AListing(), "Dada una publicación con 5 fotos")
            .And(_ => OwnerSignsIn(), "Y que su propietario inicia sesión")
            .When(_ => Removes(_fileName), "Cuando quita la primera foto")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => IsSaved(_listing), "Y se guarda la publicación sin esa foto")
            .And(_ => TheFileIsDeleted(), "Y el archivo se borra del almacenamiento")
            .BDDfy("El propietario quita una foto de su publicación");

    [TestMethod]
    public void AFileThatIsNotInTheListingIsNotDeleted() =>
        this.Given(_ => AListing(), "Dada una publicación con 5 fotos")
            .And(_ => OwnerSignsIn(), "Y que su propietario inicia sesión")
            .When(_ => Removes("00000000000000000000000000000000.jpg"), "Cuando quita una foto que no es de la publicación")
            .Then(_ => IsRejectedWith("La foto no pertenece a esta publicación."), "Entonces se rechaza: La foto no pertenece a esta publicación.")
            .And(_ => NoFileIsDeleted(), "Y no se borra ningún archivo")
            .BDDfy("No se borra del almacenamiento una foto que no es de la publicación");

    [TestMethod]
    public void TheFileNameIsRequired() =>
        this.Given(_ => AListing(), "Dada una publicación con 5 fotos")
            .And(_ => OwnerSignsIn(), "Y que su propietario inicia sesión")
            .When(_ => Removes(string.Empty), "Cuando quita una foto sin indicar cuál")
            .Then(_ => FailsValidationOn("FileName"), "Entonces la validación falla en el nombre del archivo")
            .BDDfy("Quitar una foto exige indicar el archivo");

    [TestMethod]
    public void AnotherOwnerCannotRemoveAPhoto() =>
        this.Given(_ => AListing(), "Dada una publicación con 5 fotos")
            .And(_ => AnotherOwnerSignsIn(), "Y que otro propietario inicia sesión")
            .When(_ => Removes(_fileName), "Cuando quita la primera foto")
            .Then(_ => IsForbidden(), "Entonces se rechaza por falta de permiso")
            .And(_ => NoFileIsDeleted(), "Y no se borra ningún archivo")
            .BDDfy("Otro propietario no puede quitar fotos de la publicación");

    [TestMethod]
    public void AMissingListingIsNotFound() =>
        this.Given(_ => AMissingListing(), "Dada una publicación que no existe")
            .And(_ => OwnerSignsIn(), "Y que un propietario inicia sesión")
            .When(_ => Removes(_fileName), "Cuando quita una foto")
            .Then(_ => IsNotFound(), "Entonces la publicación no se encuentra")
            .BDDfy("No se puede quitar una foto de una publicación que no existe");

    private void AListing()
    {
        AMissingListing();
        Exists(_listing);
    }

    private void AMissingListing()
    {
        _listing = ListingFactory.Complete();
        _fileName = _listing.Photos[0].FileName;
    }

    private Task Removes(string fileName)
        => TryAsync(() => Handler<RemoveListingPhotoHandler>()
            .HandleAsync(new RemoveListingPhotoCommand(_listing.Id, fileName), User, CancellationToken.None));

    private void TheFileIsDeleted()
        => PhotoStorage.Received(1).DeleteAsync(_listing.Id, _fileName, Arg.Any<CancellationToken>());

    private void NoFileIsDeleted()
        => PhotoStorage.DidNotReceiveWithAnyArgs().DeleteAsync(default, default!, default);
}
