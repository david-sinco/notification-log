using NotificationLog.RentalService.Application.Visitors.Commands.CompleteVisitorProfile;
using NotificationLog.RentalService.Domain.Common.Enums;
using NotificationLog.RentalService.Domain.Common.ValueObjects;
using NotificationLog.RentalService.Domain.Visitors;
using NotificationLog.RentalService.Domain.Visitors.Enums;
using NSubstitute;

namespace NotificationLog.RentalService.UnitTests.Application.Visitors.Commands.CompleteVisitorProfile;

[TestClass]
public class CompleteVisitorProfileHandlerTests : ApplicationScenario
{
    [TestMethod]
    public void AUserWithoutVisitorRecordBecomesARegisteredVisitor() =>
        this.Given(_ => VisitorSignsIn(), "Dado un usuario que todavía no es visitante")
            .And(_ => TheDocumentIsFree(), "Y que ningún visitante tiene ese documento")
            .When(_ => CompletesTheProfile("1020304"), "Cuando completa su perfil")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => ARegisteredVisitorIsSavedWithTheUserId(), "Y se guarda un visitante registrado con el identificador del usuario")
            .BDDfy("Un usuario sin registro de visitante queda registrado al completar su perfil");

    [TestMethod]
    public void AVisitorWithAPendingProfileCompletesIt() =>
        this.Given(_ => VisitorSignsIn(), "Dado un visitante con el perfil pendiente")
            .And(_ => TheVisitorHasAPendingProfile(), "Y que ya tiene un registro de visitante")
            .And(_ => TheDocumentIsFree(), "Y que ningún visitante tiene ese documento")
            .When(_ => CompletesTheProfile("1020304"), "Cuando completa su perfil")
            .Then(_ => ResultIs(Accepted), "Entonces se acepta")
            .And(_ => ARegisteredVisitorIsSavedWithTheUserId(), "Y se guarda ese mismo visitante como registrado")
            .BDDfy("Un visitante con el perfil pendiente lo completa");

    [TestMethod]
    public void TheProfileIsCompletedOnlyOnce() =>
        this.Given(_ => VisitorSignsIn(), "Dado un visitante")
            .And(_ => TheVisitorIsAlreadyRegistered(), "Y que ya completó su perfil")
            .When(_ => CompletesTheProfile("9080706"), "Cuando completa su perfil otra vez")
            .Then(_ => IsRejectedWith("El visitante ya completó su registro."), "Entonces se rechaza: El visitante ya completó su registro.")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .BDDfy("El perfil solo se completa una vez");

    [TestMethod]
    public void TheDocumentMustBeUnique() =>
        this.Given(_ => VisitorSignsIn(), "Dado un usuario que todavía no es visitante")
            .And(_ => TheDocumentIsTaken(), "Y que otro visitante ya tiene ese documento")
            .When(_ => CompletesTheProfile("1020304"), "Cuando completa su perfil")
            .Then(_ => IsRejectedWith("Ya existe un visitante con ese documento."), "Entonces se rechaza: Ya existe un visitante con ese documento.")
            .And(_ => NothingIsSaved(), "Y no se guarda nada")
            .BDDfy("No se admiten dos visitantes con el mismo documento");

    [TestMethod]
    public void TheDocumentNumberIsRequired() =>
        this.Given(_ => VisitorSignsIn(), "Dado un usuario que todavía no es visitante")
            .When(_ => CompletesTheProfile(string.Empty), "Cuando completa su perfil sin número de documento")
            .Then(_ => FailsValidationOn("DocumentNumber"), "Entonces la validación falla en el número de documento")
            .BDDfy("El perfil exige el número de documento");

    private void TheVisitorHasAPendingProfile() => Exists(VisitorFactory.Pending(VisitFactory.Visitor));

    private void TheVisitorIsAlreadyRegistered() => Exists(VisitorFactory.Registered(VisitFactory.Visitor));

    private void TheDocumentIsFree() => Reservation().Returns(true);

    private void TheDocumentIsTaken() => Reservation().Returns(false);

    private Task<bool> Reservation()
        => Visitors.TryReserveVisitorAsync(Arg.Any<Guid>(), Arg.Any<IdentityDocument>(), Arg.Any<CancellationToken>());

    private Task CompletesTheProfile(string documentNumber)
        => TryAsync(() => Handler<CompleteVisitorProfileHandler>().HandleAsync(
            new CompleteVisitorProfileCommand(
                "Víctor", "Rojas Peña", DocumentType.CitizenshipCard, documentNumber, "victor@example.com", "3109876543"),
            User,
            CancellationToken.None));

    private void ARegisteredVisitorIsSavedWithTheUserId()
    {
        Visitors.Received(1).AppendAsync(
            Arg.Is<Visitor>(visitor => visitor.Id == VisitFactory.Visitor && visitor.Status == VisitorStatus.Registered),
            Arg.Any<CancellationToken>());
        IsSaved();
    }
}
