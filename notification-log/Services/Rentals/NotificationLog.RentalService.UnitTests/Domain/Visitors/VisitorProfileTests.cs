using NotificationLog.RentalService.Domain.Common.Enums;
using NotificationLog.RentalService.Domain.Common.ValueObjects;
using NotificationLog.RentalService.Domain.Visitors;
using NotificationLog.RentalService.Domain.Visitors.Enums;

namespace NotificationLog.RentalService.UnitTests.Domain.Visitors;

[TestClass]
public class VisitorProfileTests : DomainScenario
{
    private Visitor _visitor = null!;

    [TestMethod]
    public void ANewVisitorHasAPendingProfile() =>
        this.When(_ => AVisitorIsRegisteredAs("  Víctor  "), "Cuando se registra un visitante llamado «  Víctor  »")
            .Then(_ => StatusIs(VisitorStatus.PendingProfile), "Entonces el visitante queda con el perfil pendiente")
            .And(_ => IsDisplayedAs("Víctor"), "Y se muestra como Víctor")
            .BDDfy("Un visitante recién registrado tiene el perfil pendiente");

    [TestMethod]
    public void CompletingTheProfileRegistersTheVisitor() =>
        this.Given(_ => AVisitorIsRegisteredAs("Víctor"), "Dado un visitante con el perfil pendiente")
            .When(_ => CompletesTheProfile(), "Cuando completa su perfil como Víctor Rojas Peña con cédula 1020304")
            .Then(_ => StatusIs(VisitorStatus.Registered), "Entonces el visitante queda registrado")
            .And(_ => IsDisplayedAs("Víctor Rojas Peña"), "Y se muestra como Víctor Rojas Peña")
            .And(_ => DocumentIs("1020304"), "Y su documento es 1020304")
            .BDDfy("Completar el perfil deja al visitante registrado");

    [TestMethod]
    public void TheProfileIsCompletedOnlyOnce() =>
        this.Given(_ => AVisitorIsRegisteredAs("Víctor"), "Dado un visitante con el perfil pendiente")
            .And(_ => CompletesTheProfile(), "Y que ya completó su perfil")
            .When(_ => CompletesTheProfile(), "Cuando completa su perfil otra vez")
            .Then(_ => IsRejectedWith("El visitante ya completó su registro."),
                "Entonces se rechaza: El visitante ya completó su registro.")
            .BDDfy("El perfil solo se completa una vez");

    private void AVisitorIsRegisteredAs(string name)
        => _visitor = Visitor.Register(Guid.NewGuid(), name, "victor@example.com", "3109876543");

    private void CompletesTheProfile()
        => Try(() => _visitor.CompleteProfile(
            PersonName.Create("Víctor", "Rojas Peña"),
            IdentityDocument.Create(DocumentType.CitizenshipCard, "1020304"),
            ContactInfo.Create("victor@example.com", "3109876543")));

    private void StatusIs(VisitorStatus expected) => Assert.AreEqual(expected, _visitor.Status);

    private void IsDisplayedAs(string name) => Assert.AreEqual(name, _visitor.DisplayName);

    private void DocumentIs(string number) => Assert.AreEqual(number, _visitor.Document?.Number);
}
