using Application.Shared.Pagination;
using Domain.Shared.Authorization;
using NotificationLog.RentalService.Application.Visits.Queries;
using NotificationLog.RentalService.Application.Visits.Queries.Dtos;
using NotificationLog.RentalService.Application.Visits.Queries.Filters;
using NSubstitute;

namespace NotificationLog.RentalService.UnitTests.Application.Visits.Queries;

[TestClass]
public class ListVisitsHandlerTests : ApplicationScenario
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _someoneElse = Guid.NewGuid();
    private readonly PageRequest _paging = new(1, 20);
    private PagedResult<VisitDto> _page = null!;

    private UserRole Role { get; set; }
    private bool Restricted { get; set; }

    [TestMethod]
    public void NonStaffUsersOnlySeeTheVisitsTheyTakePartIn() =>
        this.Given(_ => AUserWithRole(Role), "Dado un usuario con rol <role>")
            .When(_ => ListsTheVisitsOfSomeoneElse(), "Cuando lista las visitas filtrando por otro participante")
            .Then(_ => TheFilterIsRestrictedToTheUser(Restricted), "Entonces el filtro se fuerza a sus propias visitas: <restricted>")
            .WithExamples(new ExampleTable("role", "restricted")
            {
                { UserRole.Visitor, true },
                { UserRole.Propietario, true },
                { UserRole.Moderador, false },
                { UserRole.Administrador, false },
            })
            .BDDfy("Quien no es moderador ni administrador solo ve las visitas en las que participa");

    [TestMethod]
    public void TheReadModelPageIsReturnedWithItsPagingData() =>
        this.Given(_ => AUserWithRole(UserRole.Moderador), "Dado un moderador")
            .And(_ => TheReadModelHas(1), "Y que el modelo de lectura tiene 1 visita")
            .When(_ => ListsTheVisitsOfSomeoneElse(), "Cuando lista las visitas")
            .Then(_ => ThePageHas(1, 1), "Entonces la página trae 1 visita de un total de 1")
            .BDDfy("El listado de visitas devuelve la página del modelo de lectura");

    private void AUserWithRole(UserRole role)
    {
        UserIs(role, _userId);
        VisitViews.ClearReceivedCalls();
    }

    private void TheReadModelHas(int total)
        => VisitViews.ListAsync(Arg.Any<VisitFilter>(), _paging, Arg.Any<CancellationToken>())
            .Returns(((IReadOnlyList<VisitDto>)[ReadModels.Visit(Guid.NewGuid())], total));

    private async Task ListsTheVisitsOfSomeoneElse()
        => _page = await Handler<ListVisitsHandler>()
            .HandleAsync(new VisitFilter(ParticipantId: _someoneElse), _paging, User, CancellationToken.None);

    private void TheFilterIsRestrictedToTheUser(bool restricted)
        => VisitViews.Received(1).ListAsync(
            Arg.Is<VisitFilter>(filter => filter.ParticipantId == (restricted ? _userId : _someoneElse)),
            _paging,
            Arg.Any<CancellationToken>());

    private void ThePageHas(int items, int total)
    {
        Assert.HasCount(items, _page.Items);
        Assert.AreEqual(total, _page.TotalCount);
    }
}
