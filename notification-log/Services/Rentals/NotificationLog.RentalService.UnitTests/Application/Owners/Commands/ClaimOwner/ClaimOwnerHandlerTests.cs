using Domain.Shared.Authorization;
using NotificationLog.RentalService.Application.Owners.Commands.ClaimOwner;
using NotificationLog.RentalService.Domain.Owners;
using NSubstitute;

namespace NotificationLog.RentalService.UnitTests.Application.Owners.Commands.ClaimOwner;

[TestClass]
public class ClaimOwnerHandlerTests : ApplicationScenario
{
    private readonly Guid _userId = Guid.NewGuid();
    private Owner _owner = null!;

    [TestMethod]
    public void AnOwnerClaimsTheirRecord() =>
        this.Given(_ => AnUnclaimedOwner(), "Dado un propietario sin usuario con el correo ana@example.com")
            .And(_ => AnOwnerSignsInWith(OwnerFactory.Email), "Y que inicia sesión un propietario con ese correo confirmado")
            .And(_ => TheUserIsFree(), "Y que todavía no tiene propietario")
            .When(_ => Claims(), "Cuando reclama al propietario")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => TheOwnerIsRelatedToTheUser(), "Y el propietario queda relacionado con el usuario")
            .And(_ => IsSaved(), "Y se guarda el cambio")
            .BDDfy("Un propietario reclama el propietario registrado con su correo");

    [TestMethod]
    public void AUserWithAnOwnerCannotClaimAnother() =>
        this.Given(_ => AnUnclaimedOwner(), "Dado un propietario sin usuario con el correo ana@example.com")
            .And(_ => AnOwnerSignsInWith(OwnerFactory.Email), "Y que inicia sesión un propietario con ese correo confirmado")
            .And(_ => TheUserIsTaken(), "Y que ya tiene un propietario")
            .When(_ => Claims(), "Cuando reclama al propietario")
            .Then(_ => IsRejectedWith("Ya estás registrado como propietario."), "Entonces se rechaza: Ya estás registrado como propietario.")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .BDDfy("Un usuario con propietario no reclama otro");

    [TestMethod]
    public void OnlyOwnersCanClaim() =>
        this.Given(_ => AnUnclaimedOwner(), "Dado un propietario sin usuario con el correo ana@example.com")
            .And(_ => UserIs(UserRole.Visitor, _userId, OwnerFactory.Email), "Y que inicia sesión un visitante con ese correo confirmado")
            .When(_ => Claims(), "Cuando reclama al propietario")
            .Then(_ => IsForbidden(), "Entonces se rechaza por falta de permiso")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .BDDfy("Solo un usuario con rol Propietario reclama propietarios");

    [TestMethod]
    public void AMissingOwnerIsNotFound() =>
        this.Given(_ => AMissingOwner(), "Dado un propietario que no existe")
            .And(_ => AnOwnerSignsInWith(OwnerFactory.Email), "Y que inicia sesión un propietario")
            .When(_ => Claims(), "Cuando lo reclama")
            .Then(_ => IsNotFound(), "Entonces el propietario no se encuentra")
            .BDDfy("No se puede reclamar un propietario que no existe");

    private void AnUnclaimedOwner() => Exists(_owner = OwnerFactory.Registered(Guid.NewGuid(), null));

    private void AMissingOwner() => _owner = OwnerFactory.Registered(Guid.NewGuid(), null);

    private void AnOwnerSignsInWith(string email) => UserIs(UserRole.Propietario, _userId, email);

    private void TheUserIsFree() => UserReservation().Returns(true);

    private void TheUserIsTaken() => UserReservation().Returns(false);

    private Task<bool> UserReservation()
        => Owners.TryReserveUserAsync(_owner.Id, _userId, Arg.Any<CancellationToken>());

    private Task Claims()
        => TryAsync(() => Handler<ClaimOwnerHandler>().HandleAsync(new ClaimOwnerCommand(_owner.Id), User, CancellationToken.None));

    private void TheOwnerIsRelatedToTheUser()
    {
        Assert.AreEqual(_userId, _owner.RelatedUserId);
        Owners.Received(1).AppendAsync(_owner, Arg.Any<CancellationToken>());
    }
}
