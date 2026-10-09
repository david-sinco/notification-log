using NotificationLog.RentalService.Application.Listings.Commands.ResumeListing;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.UnitTests.Application.Listings.Commands.ResumeListing;

[TestClass]
[TestCategory("Application-Listings")]
public class ResumeListingHandlerTests : ApplicationScenario
{
    private Listing _listing = null!;

    [TestMethod]
    public void TheOwnerResumesTheirListing() =>
        this.Given(_ => AListing(), "Dada una publicación pausada")
            .And(_ => OwnerSignsIn(), "Y que su propietario inicia sesión")
            .When(_ => Resumes(), "Cuando la reanuda")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => StatusIs(ListingStatus.Published), "Y la publicación queda publicada")
            .And(_ => IsSaved(_listing), "Y se guarda el cambio")
            .BDDfy("El propietario reanuda su publicación");

    [TestMethod]
    public void AnotherOwnerCannotResumeTheListing() =>
        this.Given(_ => AListing(), "Dada una publicación pausada")
            .And(_ => AnotherOwnerSignsIn(), "Y que otro propietario inicia sesión")
            .When(_ => Resumes(), "Cuando la reanuda")
            .Then(_ => IsForbidden(), "Entonces se rechaza por falta de permiso")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .BDDfy("Otro propietario no puede reanudar la publicación");

    [TestMethod]
    public void AMissingListingIsNotFound() =>
        this.Given(_ => AMissingListing(), "Dada una publicación que no existe")
            .And(_ => OwnerSignsIn(), "Y que un propietario inicia sesión")
            .When(_ => Resumes(), "Cuando la reanuda")
            .Then(_ => IsNotFound(), "Entonces la publicación no se encuentra")
            .BDDfy("No se puede reanudar una publicación que no existe");

    private void AListing() => Exists(_listing = ListingFactory.InStatus(ListingStatus.Paused));

    private void AMissingListing() => _listing = ListingFactory.InStatus(ListingStatus.Paused);

    private Task Resumes()
        => TryAsync(() => Handler<ResumeListingHandler>().HandleAsync(new ResumeListingCommand(_listing.Id), User, CancellationToken.None));

    private void StatusIs(ListingStatus expected) => Assert.AreEqual(expected, _listing.Status);
}
