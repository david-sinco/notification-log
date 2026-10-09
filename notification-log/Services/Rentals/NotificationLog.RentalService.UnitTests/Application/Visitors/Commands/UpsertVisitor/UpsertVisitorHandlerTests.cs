using Domain.Shared.Authorization;
using NotificationLog.RentalService.Application.Visitors.Commands.UpsertVisitor;
using NotificationLog.RentalService.Domain.Visitors;
using NSubstitute;

namespace NotificationLog.RentalService.UnitTests.Application.Visitors.Commands.UpsertVisitor;

[TestClass]
[TestCategory("Application-Visitors")]
public class UpsertVisitorHandlerTests : ApplicationScenario
{
    private readonly Guid _userId = Guid.NewGuid();

    private UserRole? Role { get; set; }

    [TestMethod]
    public void AVisitorUserBecomesAVisitor() =>
        this.Given(_ => TheVisitorDoesNotExist(), "Dado un usuario que todavía no es visitante")
            .When(_ => ArrivesWith(UserRole.Visitor, "Víctor Rojas Peña", "victor@example.com"),
                "Cuando llegan sus datos con rol Visitor")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => AVisitorIsSavedWith("Víctor Rojas Peña", "victor@example.com"),
                "Y se guarda un visitante con el identificador del usuario y sus datos")
            .BDDfy("Un usuario con rol Visitor queda registrado como visitante");

    [TestMethod]
    public void OtherRolesAreIgnored() =>
        this.Given(_ => TheVisitorDoesNotExist(), "Dado un usuario que no es visitante")
            .When(_ => ArrivesWith(Role, "Ana Gómez", "ana@example.com"), "Cuando llegan sus datos con rol <role>")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .WithExamples(new ExampleTable("role")
            {
                { UserRole.Propietario },
                { UserRole.Moderador },
                { UserRole.Administrador },
            })
            .BDDfy("Solo los usuarios con rol Visitor son visitantes");

    [TestMethod]
    public void DataWithoutRoleUpdatesAnExistingVisitor() =>
        this.Given(_ => TheVisitorExists(), "Dado un visitante registrado")
            .When(_ => ArrivesWith(null, "", "nuevo@example.com"), "Cuando llega un correo verificado nuevo sin rol")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => AVisitorIsSavedWith("Víctor Rojas Peña", "nuevo@example.com"),
                "Y se guarda el visitante con el correo nuevo y el mismo nombre")
            .BDDfy("Los datos sin rol actualizan a un visitante que ya existe");

    [TestMethod]
    public void DataWithoutRoleDoesNotCreateAVisitor() =>
        this.Given(_ => TheVisitorDoesNotExist(), "Dado un usuario que no es visitante")
            .When(_ => ArrivesWith(null, "", "ana@example.com"), "Cuando llega un correo verificado sin rol")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .BDDfy("Los datos sin rol no crean visitantes");

    [TestMethod]
    public void UnchangedDataIsRejected() =>
        this.Given(_ => TheVisitorExists(), "Dado un visitante registrado")
            .When(_ => ArrivesWith(UserRole.Visitor, "Víctor Rojas Peña", "victor@example.com"),
                "Cuando llegan los mismos datos")
            .Then(_ => IsRejectedWith($"Los datos del visitante '{_userId}' no cambiaron."),
                "Entonces se rechaza porque los datos no cambiaron")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .BDDfy("Los datos sin cambios no se guardan");

    private void TheVisitorDoesNotExist()
        => Visitors.LoadAsync(_userId, Arg.Any<CancellationToken>()).Returns((Visitor?)null);

    private void TheVisitorExists() => Exists(VisitorFactory.Registered(_userId));

    private Task ArrivesWith(UserRole? role, string name, string email)
        => TryAsync(() => Handler<UpsertVisitorHandler>().HandleAsync(
            new UpsertVisitorCommand(_userId, role, name, email, "+573109876543"),
            CancellationToken.None));

    private void AVisitorIsSavedWith(string name, string email)
    {
        Visitors.Received(1).AppendAsync(
            Arg.Is<Visitor>(visitor => visitor.Id == _userId && visitor.Name == name && visitor.Email == email),
            Arg.Any<CancellationToken>());
        IsSaved();
    }
}
