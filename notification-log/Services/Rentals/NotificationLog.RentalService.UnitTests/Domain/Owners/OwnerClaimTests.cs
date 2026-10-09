using NotificationLog.RentalService.Domain.Owners;
using NotificationLog.RentalService.Domain.Owners.Events;

namespace NotificationLog.RentalService.UnitTests.Domain.Owners;

[TestClass]
[TestCategory("Domain-Owners")]
public class OwnerClaimTests : DomainScenario
{
    private static readonly Guid User = Guid.NewGuid();

    private Owner _owner = null!;

    [TestMethod]
    public void AUserWithTheConfirmedEmailClaimsTheOwner() =>
        this.Given(_ => AnUnclaimedOwner(), "Dado un propietario sin usuario con el correo ana@example.com")
            .When(_ => IsClaimed(User, OwnerFactory.Email, null), "Cuando lo reclama un usuario con ese correo confirmado")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => TheRelatedUserIs(User), "Y queda relacionado con ese usuario")
            .And(_ => TheHostIs(User), "Y ese usuario pasa a ser el anfitrión de sus publicaciones")
            .And(_ => TheClaimIsRecorded(), "Y se registra el reclamo")
            .BDDfy("Un usuario con el correo confirmado reclama al propietario");

    [TestMethod]
    public void AUserWithTheConfirmedPhoneClaimsTheOwner() =>
        this.Given(_ => AnUnclaimedOwner(), "Dado un propietario sin usuario con el teléfono +573001234567")
            .When(_ => IsClaimed(User, null, "+573001234567"), "Cuando lo reclama un usuario con ese teléfono confirmado")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => TheRelatedUserIs(User), "Y queda relacionado con ese usuario")
            .BDDfy("Un usuario con el teléfono confirmado reclama al propietario");

    [TestMethod]
    public void AClaimedOwnerCannotBeClaimedAgain() =>
        this.Given(_ => AnOwnerClaimedBy(User), "Dado un propietario ya reclamado")
            .When(_ => IsClaimed(Guid.NewGuid(), OwnerFactory.Email, null), "Cuando otro usuario lo reclama")
            .Then(_ => IsRejectedWith("El propietario ya fue reclamado."), "Entonces se rechaza: El propietario ya fue reclamado.")
            .And(_ => TheRelatedUserIs(User), "Y sigue relacionado con el primer usuario")
            .BDDfy("Un propietario reclamado no se puede reclamar otra vez");

    [TestMethod]
    public void TheContactMustMatch() =>
        this.Given(_ => AnUnclaimedOwner(), "Dado un propietario sin usuario con el correo ana@example.com")
            .When(_ => IsClaimed(User, "pedro@example.com", null), "Cuando lo reclama un usuario con otro correo confirmado")
            .Then(_ => IsRejectedWith("Tu correo o teléfono confirmado no coincide con el del propietario."),
                "Entonces se rechaza: Tu correo o teléfono confirmado no coincide con el del propietario.")
            .And(_ => HasNoEffect(_owner), "Y no cambia nada")
            .BDDfy("Solo reclama quien confirmó el correo o el teléfono del propietario");

    [TestMethod]
    public void AnUnclaimedOwnerIsHostedByWhoRegisteredIt() =>
        this.Given(_ => AnUnclaimedOwner(), "Dado un propietario registrado por un moderador, sin usuario")
            .Then(_ => TheHostIs(_owner.CreatedBy), "Entonces el anfitrión de sus publicaciones es quien lo registró")
            .BDDfy("Sin usuario, el anfitrión es quien registró al propietario");

    private void AnUnclaimedOwner() => _owner = OwnerFactory.Registered(Guid.NewGuid(), null);

    private void AnOwnerClaimedBy(Guid userId) => _owner = OwnerFactory.Registered(Guid.NewGuid(), userId);

    private void IsClaimed(Guid userId, string? email, string? phone) => Try(() => _owner.Claim(userId, email, phone));

    private void TheRelatedUserIs(Guid userId) => Assert.AreEqual(userId, _owner.RelatedUserId);

    private void TheHostIs(Guid userId) => Assert.AreEqual(userId, _owner.HostUserId);

    private void TheClaimIsRecorded()
        => Assert.AreEqual(User, Assert.IsInstanceOfType<OwnerClaimed>(_owner.DomainEvents.Single()).UserId);
}
