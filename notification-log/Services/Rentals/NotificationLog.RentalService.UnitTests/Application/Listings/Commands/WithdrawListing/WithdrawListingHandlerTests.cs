using NotificationLog.RentalService.Application.Listings.Commands.WithdrawListing;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.UnitTests.Application.Listings.Commands.WithdrawListing;

[TestClass]
[TestCategory("Application-Listings")]
public class WithdrawListingHandlerTests : ApplicationScenario
{
    private Listing _listing = null!;

    [TestMethod]
    public void TheOwnerWithdrawsTheirListing() =>
        this.Given(_ => AListing(), "Dada una publicación publicada")
            .And(_ => OwnerSignsIn(), "Y que su propietario inicia sesión")
            .When(_ => WithdrawsBecause("Decidí no arrendar"), "Cuando la retira con el motivo «Decidí no arrendar»")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => StatusIs(ListingStatus.Withdrawn), "Y la publicación queda retirada")
            .And(_ => IsSaved(_listing), "Y se guarda el cambio")
            .BDDfy("El propietario retira su publicación");

    [TestMethod]
    public void AModeratorWithdrawsAnyListing() =>
        this.Given(_ => AListing(), "Dada una publicación publicada")
            .And(_ => ModeratorSignsIn(), "Y que un moderador inicia sesión")
            .When(_ => WithdrawsBecause("Contenido prohibido reiterado"), "Cuando la retira por moderación")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => StatusIs(ListingStatus.Withdrawn), "Y la publicación queda retirada")
            .BDDfy("Un moderador retira cualquier publicación");

    [TestMethod]
    public void AReasonIsRequired() =>
        this.Given(_ => AListing(), "Dada una publicación publicada")
            .And(_ => OwnerSignsIn(), "Y que su propietario inicia sesión")
            .When(_ => WithdrawsBecause(string.Empty), "Cuando la retira sin motivo")
            .Then(_ => FailsValidationOn("Reason"), "Entonces la validación falla en el motivo")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .BDDfy("Retirar exige un motivo");

    [TestMethod]
    public void AnotherOwnerCannotWithdrawTheListing() =>
        this.Given(_ => AListing(), "Dada una publicación publicada")
            .And(_ => AnotherOwnerSignsIn(), "Y que otro propietario inicia sesión")
            .When(_ => WithdrawsBecause("Decidí no arrendar"), "Cuando la retira")
            .Then(_ => IsForbidden(), "Entonces se rechaza por falta de permiso")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .BDDfy("Otro propietario no puede retirar la publicación");

    [TestMethod]
    public void AMissingListingIsNotFound() =>
        this.Given(_ => AMissingListing(), "Dada una publicación que no existe")
            .And(_ => OwnerSignsIn(), "Y que un propietario inicia sesión")
            .When(_ => WithdrawsBecause("Decidí no arrendar"), "Cuando la retira")
            .Then(_ => IsNotFound(), "Entonces la publicación no se encuentra")
            .BDDfy("No se puede retirar una publicación que no existe");

    private void AListing() => Exists(_listing = ListingFactory.InStatus(ListingStatus.Published));

    private void AMissingListing() => _listing = ListingFactory.InStatus(ListingStatus.Published);

    private Task WithdrawsBecause(string reason)
        => TryAsync(() => Handler<WithdrawListingHandler>()
            .HandleAsync(new WithdrawListingCommand(_listing.Id, reason), User, CancellationToken.None));

    private void StatusIs(ListingStatus expected) => Assert.AreEqual(expected, _listing.Status);
}
