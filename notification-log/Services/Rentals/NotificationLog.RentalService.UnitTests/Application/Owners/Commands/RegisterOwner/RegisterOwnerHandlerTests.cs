using Domain.Shared.Authorization;
using NotificationLog.RentalService.Application.Owners.Commands.RegisterOwner;
using NotificationLog.RentalService.Domain.Owners;
using NSubstitute;

namespace NotificationLog.RentalService.UnitTests.Application.Owners.Commands.RegisterOwner;

[TestClass]
[TestCategory("Application-Owners")]
public class RegisterOwnerHandlerTests : ApplicationScenario
{
    private readonly Guid _userId = Guid.NewGuid();
    private Guid _ownerId;

    private UserRole Role { get; set; }
    private string Result { get; set; } = string.Empty;

    [TestMethod]
    public void AnOwnerRegistersThemselves() =>
        this.Given(_ => AUserWithRole(UserRole.Propietario), "Dado un usuario con rol Propietario que todavía no está registrado")
            .And(_ => TheContactIsFree(), "Y que ningún propietario tiene ese correo ni ese teléfono")
            .When(_ => Registers("Ana Gómez Rincón"), "Cuando se registra como propietario")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => TheOwnerIsSavedRelatedToTheUser(), "Y el propietario se guarda con un identificador nuevo, relacionado con el usuario")
            .BDDfy("Un propietario se registra a sí mismo");

    [TestMethod]
    public void StaffRegistersAnOwnerWithANewIdentifier() =>
        this.Given(_ => AUserWithRole(UserRole.Moderador), "Dado un moderador")
            .And(_ => TheContactIsFree(), "Y que ningún propietario tiene ese correo ni ese teléfono")
            .When(_ => Registers("Ana Gómez Rincón"), "Cuando registra a un propietario")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => TheOwnerIsSavedWithoutRelatedUser(), "Y el propietario se guarda con un identificador nuevo, creado por el moderador y sin usuario relacionado")
            .BDDfy("Un moderador registra a un propietario sin usuario relacionado");

    [TestMethod]
    public void OnlyOwnersAndStaffCanRegisterOwners() =>
        this.Given(_ => AUserWithRole(Role), "Dado un usuario con rol <role>")
            .And(_ => TheContactIsFree(), "Y que ningún propietario tiene ese correo ni ese teléfono")
            .When(_ => Registers("Ana Gómez Rincón"), "Cuando registra un propietario")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("role", "result")
            {
                { UserRole.Propietario, Accepted },
                { UserRole.Moderador, Accepted },
                { UserRole.Administrador, Accepted },
                { UserRole.Visitor, "Solo un administrador, un moderador o un propietario pueden registrar propietarios." },
            })
            .BDDfy("Solo un propietario, un moderador o un administrador registran propietarios");

    [TestMethod]
    public void AnOwnerCannotRegisterTwice() =>
        this.Given(_ => AUserWithRole(UserRole.Propietario), "Dado un usuario con rol Propietario")
            .And(_ => TheUserIsAlreadyAnOwner(), "Y que ya está registrado como propietario")
            .When(_ => Registers("Ana Gómez Rincón"), "Cuando se registra de nuevo")
            .Then(_ => IsRejectedWith("Ya estás registrado como propietario."), "Entonces se rechaza: Ya estás registrado como propietario.")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .BDDfy("Un propietario no puede registrarse dos veces");

    [TestMethod]
    public void TheEmailMustBeUnique() =>
        this.Given(_ => AUserWithRole(UserRole.Propietario), "Dado un usuario con rol Propietario")
            .And(_ => TheEmailIsTaken(), "Y que otro propietario ya tiene ese correo")
            .When(_ => Registers("Ana Gómez Rincón"), "Cuando se registra como propietario")
            .Then(_ => IsRejectedWith("Ya existe un propietario con ese correo."),
                "Entonces se rechaza: Ya existe un propietario con ese correo.")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .BDDfy("No se admiten dos propietarios con el mismo correo");

    [TestMethod]
    public void ThePhoneMustBeUnique() =>
        this.Given(_ => AUserWithRole(UserRole.Propietario), "Dado un usuario con rol Propietario")
            .And(_ => ThePhoneIsTaken(), "Y que otro propietario ya tiene ese teléfono")
            .When(_ => Registers("Ana Gómez Rincón"), "Cuando se registra como propietario")
            .Then(_ => IsRejectedWith("Ya existe un propietario con ese teléfono."),
                "Entonces se rechaza: Ya existe un propietario con ese teléfono.")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .BDDfy("No se admiten dos propietarios con el mismo teléfono");

    [TestMethod]
    public void TheNameIsRequired() =>
        this.Given(_ => AUserWithRole(UserRole.Propietario), "Dado un usuario con rol Propietario")
            .When(_ => Registers(string.Empty), "Cuando se registra sin nombre")
            .Then(_ => FailsValidationOn("Name"), "Entonces la validación falla en el nombre")
            .BDDfy("El registro exige el nombre");

    private void AUserWithRole(UserRole role)
    {
        UserIs(role, _userId);
        UserReservation().Returns(true);
    }

    private void TheContactIsFree()
    {
        EmailReservation().Returns(true);
        PhoneReservation().Returns(true);
    }

    private void TheEmailIsTaken()
    {
        EmailReservation().Returns(false);
        PhoneReservation().Returns(true);
    }

    private void ThePhoneIsTaken()
    {
        EmailReservation().Returns(true);
        PhoneReservation().Returns(false);
    }

    private void TheUserIsAlreadyAnOwner() => UserReservation().Returns(false);

    private Task<bool> UserReservation()
        => Owners.TryReserveUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<CancellationToken>());

    private Task<bool> EmailReservation()
        => Owners.TryReserveEmailAsync(Arg.Any<Guid>(), "ana@example.com", Arg.Any<CancellationToken>());

    private Task<bool> PhoneReservation()
        => Owners.TryReservePhoneAsync(Arg.Any<Guid>(), "+573001234567", Arg.Any<CancellationToken>());

    private Task Registers(string name)
        => TryAsync(async () => _ownerId = await Handler<RegisterOwnerHandler>().HandleAsync(
            new RegisterOwnerCommand(name, "Ana@Example.com", "3001234567"),
            User,
            CancellationToken.None));

    private void TheOwnerIsSavedRelatedToTheUser() => TheOwnerIsSavedRelatedTo(_userId);

    private void TheOwnerIsSavedWithoutRelatedUser() => TheOwnerIsSavedRelatedTo(null);

    private void TheOwnerIsSavedRelatedTo(Guid? relatedUserId)
    {
        Assert.AreNotEqual(_userId, _ownerId);
        Owners.Received(1).AppendAsync(
            Arg.Is<Owner>(owner =>
                owner.Id == _ownerId && owner.CreatedBy == _userId && owner.RelatedUserId == relatedUserId),
            Arg.Any<CancellationToken>());
        IsSaved();
    }
}
