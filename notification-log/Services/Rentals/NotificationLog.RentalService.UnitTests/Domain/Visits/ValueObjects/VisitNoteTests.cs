using NotificationLog.RentalService.Domain.Visits.ValueObjects;

namespace NotificationLog.RentalService.UnitTests.Domain.Visits.ValueObjects;

[TestClass]
public class VisitNoteTests : DomainScenario
{
    private const string HasContactData = "El mensaje de la visita no puede contener teléfonos, correos ni enlaces.";

    private string Note { get; set; } = string.Empty;
    private int Length { get; set; }
    private string Result { get; set; } = string.Empty;

    [TestMethod]
    public void TheNoteCarriesNoContactData() =>
        this.When(_ => NoteIsWritten(Note), "Cuando el visitante escribe el mensaje «<note>»")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("note", "result")
            {
                { "Prefiero ver primero el parqueadero", Accepted },
                { "Voy con 2 personas", Accepted },
                { "Escríbeme a ana@example.com", HasContactData },
                { "Llámame al 300 123 4567", HasContactData },
                { "Mira mi perfil en www.ejemplo.com", HasContactData },
                { "Más fotos en https://fotos.example/abc", HasContactData },
                { "", "El mensaje de la visita no puede estar vacío." },
            })
            .BDDfy("El mensaje de la visita no lleva datos de contacto");

    [TestMethod]
    public void TheNoteHasUpTo500Characters() =>
        this.When(_ => NoteOfLengthIsWritten(Length), "Cuando el visitante escribe un mensaje de <length> caracteres")
            .Then(_ => ResultIs(Result), "Entonces <result>")
            .WithExamples(new ExampleTable("length", "result")
            {
                { 500, Accepted },
                { 501, "El mensaje de la visita no puede superar 500 caracteres." },
            })
            .BDDfy("El mensaje de la visita tiene hasta 500 caracteres");

    private void NoteIsWritten(string note) => Try(() => VisitNote.Create(note));

    private void NoteOfLengthIsWritten(int length) => Try(() => VisitNote.Create(new string('a', length)));
}
