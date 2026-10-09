using NotificationLog.RentalService.Application.Listings.Commands.UpdateListingDetails;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;

namespace NotificationLog.RentalService.UnitTests.Application.Listings.Commands.UpdateListingDetails;

[TestClass]
[TestCategory("Application-Listings")]
public class UpdateListingDetailsHandlerTests : ApplicationScenario
{
    private Listing _listing = null!;

    private decimal Area { get; set; }
    private int Bathrooms { get; set; }
    private int Stratum { get; set; }
    private int Length { get; set; }
    private string Field { get; set; } = string.Empty;

    [TestMethod]
    public void TheOwnerUpdatesTheDetailsOfTheirListing() =>
        this.Given(_ => AListing(), "Dada una publicación en borrador sin datos")
            .And(_ => OwnerSignsIn(), "Y que su propietario inicia sesión")
            .When(_ => UpdatesWith(68m, 2, 4, 150), "Cuando indica 68 m², 2 baños, estrato 4, barrio Usaquén y una descripción de 150 caracteres")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => DetailsAreStored(), "Y la publicación queda con esos datos, ubicación y descripción")
            .And(_ => IsSaved(_listing), "Y se guarda el cambio")
            .BDDfy("El propietario actualiza los datos de su publicación");

    [TestMethod]
    public void TheCommandIsValidatedBeforeLoadingTheListing() =>
        this.Given(_ => AListing(), "Dada una publicación en borrador")
            .And(_ => OwnerSignsIn(), "Y que su propietario inicia sesión")
            .When(_ => UpdatesWith(Area, Bathrooms, Stratum, Length),
                "Cuando indica <area> m², <bathrooms> baños, estrato <stratum> y una descripción de <length> caracteres")
            .Then(_ => FailsValidationOn(Field), "Entonces la validación falla en <field>")
            .WithExamples(new ExampleTable("area", "bathrooms", "stratum", "length", "field")
            {
                { 19.99m, 2, 4, 150, "Area" },
                { 68m, 0, 4, 150, "Bathrooms" },
                { 68m, 2, 0, 150, "Stratum" },
                { 68m, 2, 7, 150, "Stratum" },
                { 68m, 2, 4, 99, "Description" },
                { 68m, 2, 4, 2001, "Description" },
            })
            .BDDfy("Los datos se validan antes de tocar la publicación");

    [TestMethod]
    public void AnotherOwnerCannotUpdateTheDetails() =>
        this.Given(_ => AListing(), "Dada una publicación en borrador")
            .And(_ => AnotherOwnerSignsIn(), "Y que otro propietario inicia sesión")
            .When(_ => UpdatesWith(68m, 2, 4, 150), "Cuando actualiza los datos")
            .Then(_ => IsForbidden(), "Entonces se rechaza por falta de permiso")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .BDDfy("Otro propietario no puede actualizar los datos de la publicación");

    [TestMethod]
    public void AMissingListingIsNotFound() =>
        this.Given(_ => AMissingListing(), "Dada una publicación que no existe")
            .And(_ => OwnerSignsIn(), "Y que un propietario inicia sesión")
            .When(_ => UpdatesWith(68m, 2, 4, 150), "Cuando actualiza los datos")
            .Then(_ => IsNotFound(), "Entonces la publicación no se encuentra")
            .BDDfy("No se pueden actualizar los datos de una publicación que no existe");

    private void AListing() => Exists(_listing = ListingFactory.Draft());

    private void AMissingListing() => _listing = ListingFactory.Draft();

    private Task UpdatesWith(decimal squareMeters, int bathroomCount, int stratumValue, int descriptionLength)
        => TryAsync(() => Handler<UpdateListingDetailsHandler>().HandleAsync(
            new UpdateListingDetailsCommand(
                _listing.Id,
                PropertyType.Apartment,
                squareMeters,
                2,
                bathroomCount,
                1,
                stratumValue,
                5,
                true,
                320_000,
                "Bogotá",
                "Usaquén",
                "Calle 120 # 7-30",
                new string('a', descriptionLength)),
            User,
            CancellationToken.None));

    private void DetailsAreStored()
    {
        Assert.AreEqual(68m, _listing.Details?.Area);
        Assert.AreEqual("Usaquén", _listing.Location?.Neighborhood);
        Assert.AreEqual(150, _listing.Description?.Value.Length);
    }
}
