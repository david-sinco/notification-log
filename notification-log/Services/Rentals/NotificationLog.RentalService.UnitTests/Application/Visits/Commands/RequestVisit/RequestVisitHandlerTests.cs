using NotificationLog.RentalService.Application.Visits.Commands.RequestVisit;
using NotificationLog.RentalService.Domain.Listings;
using NotificationLog.RentalService.Domain.Listings.Enums;
using NotificationLog.RentalService.Domain.Visits;
using NSubstitute;

namespace NotificationLog.RentalService.UnitTests.Application.Visits.Commands.RequestVisit;

[TestClass]
public class RequestVisitHandlerTests : ApplicationScenario
{
    private const string NoProfile = "sin registro de visitante";
    private const string CompleteProfile = "con registro de visitante";

    private Listing _listing = null!;
    private Guid _visitId;

    private string Profile { get; set; } = string.Empty;
    private ListingStatus Status { get; set; }
    private int Slots { get; set; }
    private string Field { get; set; } = string.Empty;
    private string Result { get; set; } = string.Empty;

    [TestMethod]
    public void ARegisteredVisitorRequestsAVisitToAPublishedListing() =>
        this.Given(_ => AListingInStatus(ListingStatus.Published), "Dada una publicación publicada")
            .And(_ => AVisitor(CompleteProfile), "Y un visitante registrado que inicia sesión")
            .When(_ => RequestsAVisitWithSlots(1), "Cuando pide una visita proponiendo una franja")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => TheVisitIsSavedBetweenHostAndVisitor(), "Y se guarda la visita entre el propietario de la publicación y el visitante")
            .And(_ => TheHostIsNotifiedWithTheVisitorName(), "Y se notifica al anfitrión la solicitud con el nombre del visitante y las franjas")
            .BDDfy("Un visitante registrado pide una visita a una publicación publicada");

    [TestMethod]
    public void OnlyARegisteredVisitorCanRequest() =>
        this.Given(_ => AListingInStatus(ListingStatus.Published), "Dada una publicación publicada")
            .And(_ => AVisitor(Profile), "Y un usuario <profile>")
            .When(_ => RequestsAVisitWithSlots(1), "Cuando pide una visita")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("profile", "result")
            {
                { CompleteProfile, Accepted },
                { NoProfile, "Solo los visitantes pueden pedir una visita." },
            })
            .BDDfy("Solo un visitante registrado puede pedir visita");

    [TestMethod]
    public void OnlyPublishedListingsCanBeVisited() =>
        this.Given(_ => AListingInStatus(Status), "Dada una publicación en estado <status>")
            .And(_ => AVisitor(CompleteProfile), "Y un visitante registrado")
            .When(_ => RequestsAVisitWithSlots(1), "Cuando pide una visita")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("status", "result")
            {
                { ListingStatus.Published, Accepted },
                { ListingStatus.Draft, "Solo se pueden visitar publicaciones publicadas." },
                { ListingStatus.Paused, "Solo se pueden visitar publicaciones publicadas." },
                { ListingStatus.Suspended, "Solo se pueden visitar publicaciones publicadas." },
            })
            .BDDfy("Solo se pueden visitar publicaciones publicadas");

    [TestMethod]
    public void SlotsAreCheckedAgainstTheClock() =>
        this.Given(_ => AListingInStatus(ListingStatus.Published), "Dada una publicación publicada")
            .And(_ => AVisitor(CompleteProfile), "Y un visitante registrado")
            .And(_ => ItIs("2026-10-06 10:01"), "Y son las 2026-10-06 10:01")
            .When(_ => RequestsAVisitWithSlots(1), "Cuando pide una visita para el 2026-10-07 10:00")
            .Then(_ => IsRejectedWith("Cada franja debe empezar en al menos 24 horas."),
                "Entonces se rechaza: Cada franja debe empezar en al menos 24 horas.")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .And(_ => NobodyIsNotified(), "Y no se notifica a nadie")
            .BDDfy("Las franjas se comparan con la hora actual");

    [TestMethod]
    public void TheCommandIsValidated() =>
        this.Given(_ => AListingInStatus(ListingStatus.Published), "Dada una publicación publicada")
            .And(_ => AVisitor(CompleteProfile), "Y un visitante registrado")
            .When(_ => RequestsAVisitWithSlots(Slots), "Cuando pide una visita proponiendo <slots> franjas")
            .Then(_ => FailsValidationOn(Field), "Entonces la validación falla en <field>")
            .WithExamples(new ExampleTable("slots", "field")
            {
                { 0, "Slots" },
                { 4, "Slots.Count" },
            })
            .BDDfy("La solicitud exige entre 1 y 3 franjas");

    [TestMethod]
    public void AMissingListingIsNotFound() =>
        this.Given(_ => AMissingListing(), "Dada una publicación que no existe")
            .And(_ => AVisitor(CompleteProfile), "Y un visitante registrado")
            .When(_ => RequestsAVisitWithSlots(1), "Cuando pide una visita")
            .Then(_ => IsNotFound(), "Entonces la publicación no se encuentra")
            .BDDfy("No se puede pedir visita a una publicación que no existe");

    private void AListingInStatus(ListingStatus status) => Exists(_listing = ListingFactory.InStatus(status));

    private void AMissingListing() => _listing = ListingFactory.InStatus(ListingStatus.Published);

    private void AVisitor(string profile)
    {
        VisitorSignsIn();

        if (profile == CompleteProfile)
            Exists(VisitorFactory.Registered(VisitFactory.Visitor));
        else
            Visitors.LoadAsync(VisitFactory.Visitor, Arg.Any<CancellationToken>()).Returns((NotificationLog.RentalService.Domain.Visitors.Visitor?)null);
    }

    private Task RequestsAVisitWithSlots(int count)
    {
        var slots = Enumerable.Range(0, count).Select(i => Clock.At("2026-10-07 10:00").AddHours(2 * i)).ToList();

        return TryAsync(async () => _visitId = await Handler<RequestVisitHandler>()
            .HandleAsync(new RequestVisitCommand(_listing.Id, slots), User, CancellationToken.None));
    }

    private void TheVisitIsSavedBetweenHostAndVisitor()
    {
        Visits.Received(1).AppendAsync(
            Arg.Is<Visit>(visit =>
                visit.Id == _visitId
                && visit.ListingId == _listing.Id
                && visit.HostId == ListingFactory.Owner
                && visit.VisitorId == VisitFactory.Visitor),
            Arg.Any<CancellationToken>());
        IsSaved();
    }

    private void TheHostIsNotifiedWithTheVisitorName()
        => Notifications.Received(1).NotifyAsync(
            VisitNotificationKeys.Requested,
            ListingFactory.Owner,
            Arg.Is<IReadOnlyDictionary<string, string>>(data =>
                data["visitor_name"] == "Víctor Rojas Peña" && data["slots"] == "2026-10-07 10:00" && data["visit_id"] == _visitId.ToString()),
            Arg.Any<CancellationToken>());
}
