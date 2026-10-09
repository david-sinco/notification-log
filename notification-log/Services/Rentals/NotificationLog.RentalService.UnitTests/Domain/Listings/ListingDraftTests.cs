using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.UnitTests.Domain.Listings;

[TestClass]
[TestCategory("Domain-Listings")]
public class ListingDraftTests : DomainScenario
{
    private Listing _listing = null!;

    [TestMethod]
    public void AListingStartsAsADraftOwnedByItsOwner() =>
        this.When(_ => AListingIsDraftedFor(ListingFactory.Owner), "Cuando se crea una publicación de arriendo para un propietario")
            .Then(_ => StatusIs(ListingStatus.Draft), "Entonces la publicación queda en borrador")
            .And(_ => BelongsToTheOwner(), "Y pertenece al propietario")
            .And(_ => HasNoPriceNorPhotos(), "Y todavía no tiene precio ni fotos")
            .BDDfy("Una publicación nace en borrador a nombre de su propietario");

    [TestMethod]
    public void AListingNeedsAnOwner() =>
        this.When(_ => AListingIsDraftedFor(Guid.Empty), "Cuando se crea una publicación sin propietario")
            .Then(_ => IsRejectedWith("El propietario de la publicación es obligatorio."),
                "Entonces se rechaza: El propietario de la publicación es obligatorio.")
            .BDDfy("Una publicación necesita propietario");

    private void AListingIsDraftedFor(Guid owner)
        => Try(() => _listing = Listing.Draft(Guid.NewGuid(), owner, ListingFactory.Owner, Operation.Rent));

    private void StatusIs(ListingStatus expected) => Assert.AreEqual(expected, _listing.Status);

    private void BelongsToTheOwner() => Assert.AreEqual(ListingFactory.Owner, _listing.OwnerId);

    private void HasNoPriceNorPhotos()
    {
        Assert.IsNull(_listing.Price);
        Assert.IsEmpty(_listing.Photos);
    }
}
