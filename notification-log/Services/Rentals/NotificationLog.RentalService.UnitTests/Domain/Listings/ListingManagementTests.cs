using System.Security.Claims;
using Domain.Shared.Authorization;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.UnitTests.Domain.Listings;

[TestClass]
public class ListingManagementTests : DomainScenario
{
    private Listing _listing = null!;
    private ClaimsPrincipal _user = null!;

    private UserRole Role { get; set; }
    private bool Owner { get; set; }
    private string Result { get; set; } = string.Empty;

    [TestMethod]
    public void OnlyTheOwnerAnAdministratorOrAModeratorManageTheListing() =>
        this.Given(_ => APublishedListing(), "Dada una publicación publicada")
            .And(_ => AUserWith(Role, Owner), "Y un usuario con rol <role> que es el propietario de la publicación: <owner>")
            .When(_ => TriesToManageTheListing(), "Cuando intenta gestionar la publicación")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("role", "owner", "result")
            {
                { UserRole.Administrador, false, Accepted },
                { UserRole.Moderador, false, Accepted },
                { UserRole.Propietario, true, Accepted },
                { UserRole.Propietario, false, "No tienes permiso para gestionar esta publicación." },
                { UserRole.Visitor, true, "No tienes permiso para gestionar esta publicación." },
                { UserRole.Visitor, false, "No tienes permiso para gestionar esta publicación." },
            })
            .BDDfy("Solo el propietario, un administrador o un moderador gestionan la publicación");

    private void APublishedListing() => _listing = ListingFactory.InStatus(ListingStatus.Published);

    private void AUserWith(UserRole role, bool isOwner)
    {
        var id = isOwner ? ListingFactory.Owner : Guid.NewGuid();

        _user = TestUser.With(role, id);
    }

    private void TriesToManageTheListing() => Try(() => _listing.EnsureCanManage(_user, ListingFactory.Owner));
}
