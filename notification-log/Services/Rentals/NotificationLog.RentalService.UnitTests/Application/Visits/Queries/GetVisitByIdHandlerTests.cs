using Domain.Shared.Authorization;
using NotificationLog.RentalService.Application.Visits.Queries;
using NSubstitute;

namespace NotificationLog.RentalService.UnitTests.Application.Visits.Queries;

[TestClass]
[TestCategory("Application-Visits")]
public class GetVisitByIdHandlerTests : ApplicationScenario
{
    private const string Host = "el anfitrión";
    private const string Visitor = "el visitante";
    private const string ThirdParty = "un tercero";
    private const string Moderator = "un moderador";
    private const string Administrator = "un administrador";

    private readonly Guid _visitId = Guid.NewGuid();

    private string Who { get; set; } = string.Empty;
    private string Result { get; set; } = string.Empty;

    [TestMethod]
    public void OnlyParticipantsAndStaffCanQueryAVisit() =>
        this.Given(_ => AVisitInTheReadModel(), "Dada una visita en el modelo de lectura")
            .And(_ => SignsIn(Who), "Y que <who> inicia sesión")
            .When(_ => IsQueried(), "Cuando consulta la visita")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("who", "result")
            {
                { Host, Accepted },
                { Visitor, Accepted },
                { Moderator, Accepted },
                { Administrator, Accepted },
                { ThirdParty, "Solo puedes consultar las visitas en las que participas." },
            })
            .BDDfy("Solo los participantes, un moderador o un administrador consultan una visita");

    [TestMethod]
    public void AMissingVisitIsNotFound() =>
        this.Given(_ => SignsIn(Moderator), "Dado un moderador")
            .When(_ => IsQueried(), "Cuando consulta una visita que no existe")
            .Then(_ => IsNotFound(), "Entonces la visita no se encuentra")
            .BDDfy("Una visita que no existe no se encuentra");

    private void AVisitInTheReadModel()
        => VisitViews.GetAsync(_visitId, Arg.Any<CancellationToken>()).Returns(ReadModels.Visit(_visitId));

    private void SignsIn(string who)
    {
        switch (who)
        {
            case Host:
                HostSignsIn();
                break;
            case Visitor:
                VisitorSignsIn();
                break;
            case Moderator:
                ModeratorSignsIn();
                break;
            case Administrator:
                UserIs(UserRole.Administrador, Guid.NewGuid());
                break;
            default:
                AThirdPartySignsIn();
                break;
        }
    }

    private Task IsQueried()
        => TryAsync(() => Handler<GetVisitByIdHandler>().HandleAsync(_visitId, User, CancellationToken.None));
}
