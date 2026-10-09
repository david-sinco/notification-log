using NotificationLog.RentalService.Application.Listings.Queries;
using NotificationLog.RentalService.Application.Listings.Queries.Dtos;
using NSubstitute;

namespace NotificationLog.RentalService.UnitTests.Application.Listings.Queries;

[TestClass]
[TestCategory("Application-Listings")]
public class GetListingByIdHandlerTests : ApplicationScenario
{
    private readonly Guid _id = Guid.NewGuid();
    private ListingDto? _found;

    [TestMethod]
    public void AnExistingListingIsReturned() =>
        this.Given(_ => AListingInTheReadModel(), "Dada una publicación en el modelo de lectura")
            .When(_ => IsQueried(), "Cuando se consulta por su identificador")
            .Then(_ => TheListingIsReturned(), "Entonces se devuelve la publicación")
            .BDDfy("Se obtiene una publicación existente");

    [TestMethod]
    public void AMissingListingIsNotFound() =>
        this.When(_ => IsQueried(), "Cuando se consulta una publicación que no existe")
            .Then(_ => IsNotFound(), "Entonces la publicación no se encuentra")
            .BDDfy("Una publicación que no existe no se encuentra");

    private void AListingInTheReadModel()
        => ListingViews.GetAsync(_id, Arg.Any<CancellationToken>()).Returns(ReadModels.Listing(_id));

    private Task IsQueried()
        => TryAsync(async () => _found = await Handler<GetListingByIdHandler>().HandleAsync(_id, CancellationToken.None));

    private void TheListingIsReturned() => Assert.AreEqual(_id, _found?.Id);
}
