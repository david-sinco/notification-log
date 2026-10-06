using Domain.Shared.Authorization;
using NotificationLog.RentalService.Application.Owners.Queries;
using NSubstitute;

namespace NotificationLog.RentalService.UnitTests.Application.Owners.Queries;

[TestClass]
public class GetOwnerByIdHandlerTests : ApplicationScenario
{
    private readonly Guid _ownerId = Guid.NewGuid();
    private readonly Guid _registeredBy = Guid.NewGuid();

    private UserRole Role { get; set; }
    private bool Creator { get; set; }
    private string Result { get; set; } = string.Empty;

    [TestMethod]
    public void OnlyTheCreatorAndStaffCanQueryAnOwner() =>
        this.Given(_ => AnOwnerInTheReadModel(), "Dado un propietario en el modelo de lectura")
            .And(_ => AUser(Role, Creator), "Y un usuario con rol <role> que lo registró: <creator>")
            .When(_ => IsQueried(), "Cuando consulta el propietario")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("role", "creator", "result")
            {
                { UserRole.Propietario, true, Accepted },
                { UserRole.Propietario, false, "Solo puedes consultar los propietarios que registraste." },
                { UserRole.Visitor, false, "Solo puedes consultar los propietarios que registraste." },
                { UserRole.Moderador, false, Accepted },
                { UserRole.Administrador, false, Accepted },
            })
            .BDDfy("Solo quien registró al propietario, un moderador o un administrador pueden consultarlo");

    [TestMethod]
    public void AMissingOwnerIsNotFound() =>
        this.Given(_ => AUser(UserRole.Moderador, false), "Dado un moderador")
            .When(_ => IsQueried(), "Cuando consulta un propietario que no existe")
            .Then(_ => IsNotFound(), "Entonces el propietario no se encuentra")
            .BDDfy("Un propietario que no existe no se encuentra");

    private void AnOwnerInTheReadModel()
        => OwnerViews.GetAsync(_ownerId, Arg.Any<CancellationToken>()).Returns(ReadModels.Owner(_ownerId, _registeredBy));

    private void AUser(UserRole role, bool isCreator) => UserIs(role, isCreator ? _registeredBy : Guid.NewGuid());

    private Task IsQueried()
        => TryAsync(() => Handler<GetOwnerByIdHandler>().HandleAsync(_ownerId, User, CancellationToken.None));
}
