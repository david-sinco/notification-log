using NotificationLog.RentalService.Domain.Common.ValueObjects;
using NotificationLog.RentalService.Domain.Listings.Enums;
using NotificationLog.RentalService.Domain.Listings.ValueObjects;

namespace NotificationLog.RentalService.UnitTests.Domain.Listings.ValueObjects;

[TestClass]
public class PropertyDetailsTests : DomainScenario
{
    private decimal Area { get; set; }
    private int Bedrooms { get; set; }
    private int Bathrooms { get; set; }
    private int Parking { get; set; }
    private int Floor { get; set; }
    private string Result { get; set; } = string.Empty;

    [TestMethod]
    public void PropertyDetailsHaveMinimumValues() =>
        this.When(_ => DetailsAreCreated(Area, Bedrooms, Bathrooms, Parking, Floor),
                "Cuando se indican <area> m², <bedrooms> habitaciones, <bathrooms> baños, <parking> parqueaderos y piso <floor>")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("area", "bedrooms", "bathrooms", "parking", "floor", "result")
            {
                { 20m, 0, 1, 0, 1, Accepted },
                { 68m, 2, 2, 1, 5, Accepted },
                { 19.99m, 2, 2, 1, 5, "El área debe ser de al menos 20 m²." },
                { 68m, -1, 2, 1, 5, "El número de habitaciones no puede ser negativo." },
                { 68m, 2, 0, 1, 5, "El inmueble debe tener al menos un baño." },
                { 68m, 2, 2, -1, 5, "El número de parqueaderos no puede ser negativo." },
                { 68m, 2, 2, 1, 0, "El piso debe ser 1 o mayor." },
            })
            .BDDfy("Los datos del inmueble tienen valores mínimos");

    [TestMethod]
    public void TheFloorIsOptional() =>
        this.When(_ => DetailsWithoutFloorAreCreated(), "Cuando se indican los datos de un inmueble sin piso")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .BDDfy("El piso es opcional");

    private void DetailsAreCreated(decimal squareMeters, int bedroomCount, int bathroomCount, int parkingSpots, int level)
        => Create(squareMeters, bedroomCount, bathroomCount, parkingSpots, level);

    private void DetailsWithoutFloorAreCreated() => Create(68m, 2, 2, 1, null);

    private void Create(decimal area, int bedrooms, int bathrooms, int parkingSpots, int? floor)
        => Try(() => PropertyDetails.Create(
            PropertyType.Apartment,
            area,
            bedrooms,
            bathrooms,
            parkingSpots,
            Stratum.Create(4),
            floor,
            true,
            Money.Create(320_000)));
}
