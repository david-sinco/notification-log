using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var app = builder.Build();

app.MapDefaultEndpoints();

// Buzón en memoria: es una herramienta de dev, no necesita sobrevivir un reinicio.
var inbox = new ConcurrentStack<SmsMessage>();

// Forma de request tipo Twilio (to/from/body) para que un futuro ISmsNotificationSender apunte
// acá con el mismo shape que usaría contra un proveedor real, y solo cambie la base URL.
app.MapPost("/messages", (SendSmsRequest request) =>
{
    var message = new SmsMessage(
        Guid.NewGuid().ToString("n"),
        DateTimeOffset.UtcNow,
        request.To,
        request.From,
        request.Body);

    inbox.Push(message);

    return Results.Created($"/api/messages/{message.Id}", new { sid = message.Id, status = "queued" });
});

app.MapGet("/api/messages", () => inbox.OrderByDescending(m => m.ReceivedAt));

app.MapDelete("/api/messages", () =>
{
    inbox.Clear();
    return Results.NoContent();
});

app.MapGet("/", () => Results.Content(IndexHtml, "text/html"));

app.Run();

internal sealed record SendSmsRequest(string To, string From, string Body);

internal sealed record SmsMessage(string Id, DateTimeOffset ReceivedAt, string To, string From, string Body);

internal static partial class Program
{
    public const string IndexHtml = """
        <!doctype html>
        <html lang="es">
        <head>
        <meta charset="utf-8" />
        <title>SmsSink</title>
        <style>
          body { font-family: system-ui, sans-serif; margin: 2rem; background: #0f172a; color: #e2e8f0; }
          h1 { font-size: 1.25rem; }
          table { width: 100%; border-collapse: collapse; margin-top: 1rem; }
          th, td { text-align: left; padding: .5rem; border-bottom: 1px solid #334155; vertical-align: top; }
          th { color: #94a3b8; font-weight: 600; font-size: .85rem; }
          tr:hover { background: #1e293b; }
          .empty { color: #64748b; margin-top: 1rem; }
        </style>
        </head>
        <body>
        <h1>SmsSink — mensajes SMS capturados (dev)</h1>
        <table id="tbl"><thead><tr><th>Recibido</th><th>De</th><th>Para</th><th>Mensaje</th></tr></thead><tbody></tbody></table>
        <p class="empty" id="empty" hidden>Sin mensajes todavía.</p>
        <script>
        async function refresh() {
          const res = await fetch('/api/messages');
          const messages = await res.json();
          const body = document.querySelector('#tbl tbody');
          const empty = document.querySelector('#empty');
          body.innerHTML = '';
          empty.hidden = messages.length > 0;
          for (const m of messages) {
            const tr = document.createElement('tr');
            const cells = [new Date(m.receivedAt).toLocaleString(), m.from, m.to, m.body];
            for (const text of cells) {
              const td = document.createElement('td');
              td.textContent = text;
              tr.appendChild(td);
            }
            body.appendChild(tr);
          }
        }
        refresh();
        setInterval(refresh, 2000);
        </script>
        </body>
        </html>
        """;
}
