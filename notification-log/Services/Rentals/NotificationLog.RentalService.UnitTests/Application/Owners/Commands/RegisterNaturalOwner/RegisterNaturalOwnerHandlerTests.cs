using Domain.Shared.Authorization;
using NotificationLog.RentalService.Application.Owners.Commands.RegisterNaturalOwner;
using NotificationLog.RentalService.Domain.Common.Enums;
using NotificationLog.RentalService.Domain.Common.ValueObjects;
using NotificationLog.RentalService.Domain.Owners;
using NSubstitute;

namespace NotificationLog.RentalService.UnitTests.Application.Owners.Commands.RegisterNaturalOwner;

[TestClass]
public class RegisterNaturalOwnerHandlerTests : ApplicationScenario
{
    private readonly Guid _userId = Guid.NewGuid();
    private Guid _ownerId;

    private UserRole Role { get; set; }
    private string Result { get; set; } = string.Empty;

    [TestMethod]
    public void AnOwnerRegistersThemselves() =>
        this.Given(_ => AUserWithRole(UserRole.Propietario), "Dado un usuario con rol Propietario que todavía no está registrado")
            .And(_ => TheDocumentIsFree(), "Y que ningún propietario tiene ese documento")
            .When(_ => Registers("Ana"), "Cuando se registra como persona natural")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => TheOwnerIsSavedRelatedToTheUser(), "Y el propietario se guarda con un identificador nuevo, relacionado con el usuario")
            .BDDfy("Un propietario se registra a sí mismo");

    [TestMethod]
    public void StaffRegistersAnOwnerWithANewIdentifier() =>
        this.Given(_ => AUserWithRole(UserRole.Moderador), "Dado un moderador")
            .And(_ => TheDocumentIsFree(), "Y que ningún propietario tiene ese documento")
            .When(_ => Registers("Ana"), "Cuando registra a un propietario persona natural")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => TheOwnerIsSavedWithoutRelatedUser(), "Y el propietario se guarda con un identificador nuevo, creado por el moderador y sin usuario relacionado")
            .BDDfy("Un moderador registra a un propietario sin usuario relacionado");

    [TestMethod]
    public void OnlyOwnersAndStaffCanRegisterOwners() =>
        this.Given(_ => AUserWithRole(Role), "Dado un usuario con rol <role>")
            .And(_ => TheDocumentIsFree(), "Y que ningún propietario tiene ese documento")
            .When(_ => Registers("Ana"), "Cuando registra un propietario persona natural")
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
            .When(_ => Registers("Ana"), "Cuando se registra de nuevo")
            .Then(_ => IsRejectedWith("Ya estás registrado como propietario."), "Entonces se rechaza: Ya estás registrado como propietario.")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .BDDfy("Un propietario no puede registrarse dos veces");

    [TestMethod]
    public void TheDocumentMustBeUnique() =>
        this.Given(_ => AUserWithRole(UserRole.Propietario), "Dado un usuario con rol Propietario")
            .And(_ => TheDocumentIsTaken(), "Y que otro propietario ya tiene ese documento")
            .When(_ => Registers("Ana"), "Cuando se registra como persona natural")
            .Then(_ => IsRejectedWith("Ya existe un propietario con ese documento."),
                "Entonces se rechaza: Ya existe un propietario con ese documento.")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .BDDfy("No se admiten dos propietarios con el mismo documento");

    [TestMethod]
    public void TheNamesAreRequired() =>
        this.Given(_ => AUserWithRole(UserRole.Propietario), "Dado un usuario con rol Propietario")
            .When(_ => Registers(string.Empty), "Cuando se registra sin nombres")
            .Then(_ => FailsValidationOn("FirstNames"), "Entonces la validación falla en los nombres")
            .BDDfy("El registro exige los nombres");

    private void AUserWithRole(UserRole role)
    {
        UserIs(role, _userId);
        UserReservation().Returns(true);
    }

    private void TheDocumentIsFree() => Reservation().Returns(true);

    private void TheDocumentIsTaken() => Reservation().Returns(false);

    private void TheUserIsAlreadyAnOwner() => UserReservation().Returns(false);

    private Task<bool> UserReservation()
        => Owners.TryReserveUserAsync(Arg.Any<Guid>(), _userId, Arg.Any<CancellationToken>());

    private Task<bool> Reservation()
        => Owners.TryReserveNaturalAsync(Arg.Any<Guid>(), Arg.Any<IdentityDocument>(), Arg.Any<CancellationToken>());

    private Task Registers(string firstNames)
        => TryAsync(async () => _ownerId = await Handler<RegisterNaturalOwnerHandler>().HandleAsync(
            new RegisterNaturalOwnerCommand(
                firstNames, "Gómez Rincón", DocumentType.CitizenshipCard, "52123456", "Ana@Example.com", "3001234567"),
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
