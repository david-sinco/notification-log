using Domain.Shared.Authorization;
using NotificationLog.RentalService.Application.Visitors.Queries;
using NSubstitute;

namespace NotificationLog.RentalService.UnitTests.Application.Visitors.Queries;

[TestClass]
public class GetVisitorByIdHandlerTests : ApplicationScenario
{
    private readonly Guid _visitorId = Guid.NewGuid();

    private UserRole Role { get; set; }
    private bool Self { get; set; }
    private string Result { get; set; } = string.Empty;

    [TestMethod]
    public void OnlyTheVisitorAndStaffCanQueryAVisitor() =>
        this.Given(_ => AVisitorInTheReadModel(), "Dado un visitante en el modelo de lectura")
            .And(_ => AUser(Role, Self), "Y un usuario con rol <role> que es ese visitante: <self>")
            .When(_ => IsQueried(), "Cuando consulta el registro del visitante")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("role", "self", "result")
            {
                { UserRole.Visitor, true, Accepted },
                { UserRole.Visitor, false, "Solo puedes consultar tu propio registro de visitante." },
                { UserRole.Propietario, false, "Solo puedes consultar tu propio registro de visitante." },
                { UserRole.Moderador, false, Accepted },
                { UserRole.Administrador, false, Accepted },
            })
            .BDDfy("Solo el propio visitante, un moderador o un administrador consultan su registro");

    [TestMethod]
    public void AnotherVisitorsRecordIsNotEvenLoaded() =>
        this.Given(_ => AVisitorInTheReadModel(), "Dado un visitante en el modelo de lectura")
            .And(_ => AUser(UserRole.Visitor, false), "Y otro visitante que inicia sesión")
            .When(_ => IsQueried(), "Cuando consulta el registro ajeno")
            .Then(_ => IsForbidden(), "Entonces se rechaza por falta de permiso")
            .And(_ => TheReadModelIsNotQueried(), "Y el modelo de lectura no llega a consultarse")
            .BDDfy("El registro de otro visitante ni siquiera se carga");

    [TestMethod]
    public void AMissingVisitorIsNotFound() =>
        this.Given(_ => AUser(UserRole.Visitor, true), "Dado un usuario que todavía no es visitante")
            .When(_ => IsQueried(), "Cuando consulta su registro de visitante")
            .Then(_ => IsNotFound(), "Entonces el registro no se encuentra")
            .BDDfy("Un visitante que no existe no se encuentra");

    private void AVisitorInTheReadModel()
        => VisitorViews.GetAsync(_visitorId, Arg.Any<CancellationToken>()).Returns(ReadModels.VisitorDetail(_visitorId));

    private void AUser(UserRole role, bool isTheVisitor) => UserIs(role, isTheVisitor ? _visitorId : Guid.NewGuid());

    private Task IsQueried()
        => TryAsync(() => Handler<GetVisitorByIdHandler>().HandleAsync(_visitorId, User, CancellationToken.None));

    private void TheReadModelIsNotQueried() => VisitorViews.DidNotReceiveWithAnyArgs().GetAsync(default, default);
}
