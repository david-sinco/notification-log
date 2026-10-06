using Domain.Shared.Authorization;
using NotificationLog.Contracts.Identity;
using NotificationLog.RentalService.Application.Owners.Commands.RegisterCompanyOwner;
using NotificationLog.RentalService.Domain.Owners;
using NotificationLog.RentalService.Domain.Owners.ValueObjects;
using NSubstitute;

namespace NotificationLog.RentalService.UnitTests.Application.Owners.Commands.RegisterCompanyOwner;

[TestClass]
public class RegisterCompanyOwnerHandlerTests : ApplicationScenario
{
    private readonly Guid _userId = Guid.NewGuid();
    private Guid _ownerId;

    [TestMethod]
    public void AnOwnerRegistersTheirCompany() =>
        this.Given(_ => AUserWithRole(UserRole.Propietario), "Dado un usuario con rol Propietario que todavía no está registrado")
            .And(_ => TheNitIsFree(), "Y que ningún propietario tiene ese NIT")
            .When(_ => Registers("900123456-8"), "Cuando se registra como persona jurídica con NIT 900123456-8")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => TheOwnerIsSavedWithTheUserId(), "Y el propietario se guarda con el identificador del usuario")
            .And(_ => AnAccountIsRequested(), "Y se solicita una cuenta con rol Propietario a nombre de la razón social")
            .BDDfy("Un propietario registra su empresa");

    [TestMethod]
    public void AVisitorCannotRegisterACompany() =>
        this.Given(_ => AUserWithRole(UserRole.Visitor), "Dado un usuario con rol Visitor")
            .When(_ => Registers("900123456-8"), "Cuando registra un propietario persona jurídica")
            .Then(_ => IsForbidden(), "Entonces se rechaza por falta de permiso")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .BDDfy("Un visitante no puede registrar propietarios");

    [TestMethod]
    public void AnOwnerCannotRegisterTwice() =>
        this.Given(_ => AUserWithRole(UserRole.Propietario), "Dado un usuario con rol Propietario")
            .And(_ => TheUserIsAlreadyAnOwner(), "Y que ya está registrado como propietario")
            .When(_ => Registers("900123456-8"), "Cuando se registra de nuevo")
            .Then(_ => IsRejectedWith("Ya estás registrado como propietario."), "Entonces se rechaza: Ya estás registrado como propietario.")
            .BDDfy("Un propietario no puede registrarse dos veces");

    [TestMethod]
    public void TheNitMustBeUnique() =>
        this.Given(_ => AUserWithRole(UserRole.Propietario), "Dado un usuario con rol Propietario")
            .And(_ => TheNitIsTaken(), "Y que otro propietario ya tiene ese NIT")
            .When(_ => Registers("900123456-8"), "Cuando se registra como persona jurídica")
            .Then(_ => IsRejectedWith("Ya existe un propietario con ese NIT."), "Entonces se rechaza: Ya existe un propietario con ese NIT.")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .And(_ => NoAccountIsRequested(), "Y no se solicita ninguna cuenta")
            .BDDfy("No se admiten dos propietarios con el mismo NIT");

    [TestMethod]
    public void TheNitIsRequired() =>
        this.Given(_ => AUserWithRole(UserRole.Propietario), "Dado un usuario con rol Propietario")
            .When(_ => Registers(string.Empty), "Cuando se registra sin NIT")
            .Then(_ => FailsValidationOn("Nit"), "Entonces la validación falla en el NIT")
            .BDDfy("El registro exige el NIT");

    private void AUserWithRole(UserRole role) => UserIs(role, _userId);

    private void TheNitIsFree() => Reservation().Returns(true);

    private void TheNitIsTaken() => Reservation().Returns(false);

    private void TheUserIsAlreadyAnOwner() => Exists(OwnerFactory.Natural(_userId));

    private Task<bool> Reservation()
        => Owners.TryReserveCompanyAsync(Arg.Any<Guid>(), Arg.Any<Nit>(), Arg.Any<CancellationToken>());

    private Task Registers(string nit)
        => TryAsync(async () => _ownerId = await Handler<RegisterCompanyOwnerHandler>().HandleAsync(
            new RegisterCompanyOwnerCommand("Inmobiliaria Andes", nit, "andes@example.com", "6011234567"),
            User,
            CancellationToken.None));

    private void TheOwnerIsSavedWithTheUserId()
    {
        Assert.AreEqual(_userId, _ownerId);
        Owners.Received(1).AppendAsync(
            Arg.Is<Owner>(owner => owner.Id == _userId && owner.CreatedBy == _userId), Arg.Any<CancellationToken>());
        IsSaved();
    }

    private void AnAccountIsRequested()
        => Accounts.Received(1).RequestAccountAsync(
            Arg.Is<AccountCreationRequested>(request =>
                request.UserId == _ownerId.ToString()
                && request.Email == "andes@example.com"
                && request.Phone == "+576011234567"
                && request.Name == "Inmobiliaria Andes"
                && request.Role == nameof(UserRole.Propietario)),
            Arg.Any<CancellationToken>());

    private void NoAccountIsRequested() => Accounts.DidNotReceiveWithAnyArgs().RequestAccountAsync(default!, default);
}
