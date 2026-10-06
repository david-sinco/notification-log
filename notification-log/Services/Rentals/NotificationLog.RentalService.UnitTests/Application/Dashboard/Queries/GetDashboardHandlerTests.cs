using Domain.Shared.Authorization;
using NotificationLog.RentalService.Application.Dashboard.Queries;
using NSubstitute;

namespace NotificationLog.RentalService.UnitTests.Application.Dashboard.Queries;

[TestClass]
public class GetDashboardHandlerTests : ApplicationScenario
{
    private Guid _userId;

    private UserRole Role { get; set; }
    private bool Staff { get; set; }

    [TestMethod]
    public void TheDashboardIsBuiltForTheUserAndTheirStaffStatus() =>
        this.Given(_ => AUserWithRole(Role), "Dado un usuario con rol <role>")
            .When(_ => TheDashboardIsQueried(), "Cuando consulta su panel")
            .Then(_ => TheReadModelIsQueriedAsStaff(Staff), "Entonces el panel se pide para ese usuario, como personal interno: <staff>")
            .WithExamples(new ExampleTable("role", "staff")
            {
                { UserRole.Administrador, true },
                { UserRole.Moderador, true },
                { UserRole.Propietario, false },
                { UserRole.Visitor, false },
            })
            .BDDfy("El panel se construye para el usuario según sea o no personal interno");

    private void AUserWithRole(UserRole role) => UserIs(role, _userId = Guid.NewGuid());

    private Task TheDashboardIsQueried() => Handler<GetDashboardHandler>().HandleAsync(User, CancellationToken.None);

    private void TheReadModelIsQueriedAsStaff(bool isStaff)
        => Dashboard.Received(1).GetAsync(_userId, isStaff, Arg.Any<CancellationToken>());
}
