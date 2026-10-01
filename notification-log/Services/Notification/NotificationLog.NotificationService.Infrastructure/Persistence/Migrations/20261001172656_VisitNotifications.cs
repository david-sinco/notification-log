using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificationLog.NotificationService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class VisitNotifications : Migration
    {
        private const int Email = 1;
        private const int Sms = 2;

        private const string Font = "font-family:'Segoe UI',Roboto,Helvetica,Arial,sans-serif;";

        private const string SmsListing =
            "{{ listing_type | string.downcase }} en {{ listing_operation }} de {{ listing_address }}, {{ listing_neighborhood }}, {{ listing_city }}";

        private const string SlotsHighlight =
            """
            <div style="font-size:12px; font-weight:600; color:#8f3f1f; text-transform:uppercase; letter-spacing:0.12em; padding-bottom:8px;">Franjas propuestas · hora de Colombia</div>
            {{ for slot in slots | string.split ", " }}<div style="font-size:18px; font-weight:700; color:#8f3f1f; line-height:1.6;">{{ slot }}</div>{{ end }}
            """;

        private const string DateHighlight =
            """
            <div style="font-size:12px; font-weight:600; color:#8f3f1f; text-transform:uppercase; letter-spacing:0.12em; padding-bottom:8px;">Fecha de la visita · hora de Colombia</div>
            <div style="font-size:22px; font-weight:700; color:#8f3f1f;">{{ starts_at }}</div>
            """;

        private const string ReasonHighlight =
            """
            <div style="font-size:12px; font-weight:600; color:#8f3f1f; text-transform:uppercase; letter-spacing:0.12em; padding-bottom:8px;">Motivo</div>
            <div style="font-size:15px; line-height:1.6; color:#1d2426;">{{ reason | html.escape }}</div>
            """;

        private static readonly (int Number, string EventKey, string Name, string Description, string Subject, string EmailBody, string SmsBody)[] Seeds =
        {
            (
                1,
                "visita.solicitada",
                "visita_solicitada",
                "Un visitante pide una visita a una publicación; se avisa al anfitrión.",
                "Nueva solicitud de visita · {{ listing_type }} en {{ listing_neighborhood }}, {{ listing_city }}",
                EmailBody(
                    "Quieren visitar tu inmueble",
                    "<strong>{{ visitor_name | html.escape }}</strong> quiere visitar tu inmueble y propone las siguientes franjas, de una hora cada una.",
                    SlotsHighlight,
                    "Entra a Llave para agendar una de las franjas o proponer otras. Si no respondes a tiempo, la solicitud vence."),
                "Llave: {{ visitor_name }} quiere visitar tu " + SmsListing + ". Franjas (hora de Colombia): {{ slots }}. Responde en Llave antes de que venza."
            ),
            (
                2,
                "visita.contrapropuesta",
                "visita_contrapropuesta",
                "Una de las partes propone otras franjas para la visita; se avisa a la otra parte.",
                "Nuevas franjas para la visita · {{ listing_type }} en {{ listing_neighborhood }}, {{ listing_city }}",
                EmailBody(
                    "Te propusieron nuevas franjas",
                    "La otra parte no puede en las franjas anteriores y propone estas, de una hora cada una, para la visita a este inmueble.",
                    SlotsHighlight,
                    "Entra a Llave para agendar una de las franjas o proponer otras. Si no respondes a tiempo, la solicitud vence."),
                "Llave: te propusieron nuevas franjas para la visita al " + SmsListing + ": {{ slots }} (hora de Colombia). Responde en Llave antes de que venza."
            ),
            (
                3,
                "visita.agendada",
                "visita_agendada",
                "Una de las partes acepta una franja y la visita queda agendada; se avisa a quien la propuso.",
                "Visita agendada para el {{ starts_at }} · {{ listing_type }} en {{ listing_neighborhood }}, {{ listing_city }}",
                EmailBody(
                    "Tu visita quedó agendada",
                    "Aceptaron una de las franjas que propusiste. La visita a este inmueble ya tiene fecha y dura una hora.",
                    DateHighlight,
                    "Si no puedes asistir, cancela la visita desde Llave con anticipación para que la otra parte no pierda el viaje."),
                "Llave: visita agendada para el {{ starts_at }} (hora de Colombia) al " + SmsListing + ". Si no puedes asistir, cancélala en Llave."
            ),
            (
                4,
                "visita.cancelada",
                "visita_cancelada",
                "Una de las partes cancela la visita; se avisa a la otra parte.",
                "Visita cancelada · {{ listing_type }} en {{ listing_neighborhood }}, {{ listing_city }}",
                EmailBody(
                    "La visita fue cancelada",
                    "La otra parte canceló la visita a este inmueble, así que ya no tienes que asistir ni responder.",
                    ReasonHighlight,
                    "Si el inmueble sigue publicado, se puede pedir una nueva visita desde Llave."),
                "Llave: cancelaron la visita al " + SmsListing + ". Motivo: {{ reason }}"
            ),
            (
                5,
                "visita.realizada",
                "visita_realizada",
                "El anfitrión marca la visita como realizada; se avisa al visitante.",
                "Visita realizada · {{ listing_type }} en {{ listing_neighborhood }}, {{ listing_city }}",
                EmailBody(
                    "Tu visita fue realizada",
                    "El anfitrión marcó como realizada tu visita a este inmueble.",
                    DateHighlight,
                    "Gracias por usar Llave. Si necesitas volver a ver el inmueble, puedes pedir otra visita."),
                "Llave: el anfitrión marcó como realizada tu visita del {{ starts_at }} al " + SmsListing + "."
            ),
            (
                6,
                "visita.inasistencia",
                "visita_inasistencia",
                "El anfitrión marca que el visitante no asistió a la visita; se avisa al visitante.",
                "Inasistencia registrada · {{ listing_type }} en {{ listing_neighborhood }}, {{ listing_city }}",
                EmailBody(
                    "Registramos una inasistencia",
                    "El anfitrión indicó que no asististe a la visita que tenías agendada a este inmueble.",
                    DateHighlight,
                    "Si todavía te interesa el inmueble, puedes pedir una nueva visita desde Llave."),
                "Llave: el anfitrión indicó que no asististe a la visita del {{ starts_at }} al " + SmsListing + "."
            )
        };

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var createdAt = new DateTime(2026, 10, 1, 0, 0, 0, DateTimeKind.Utc);

            foreach (var seed in Seeds)
            {
                migrationBuilder.InsertData(
                    table: "NotificationTemplates",
                    columns: new[] { "Id", "Name", "Channel", "IsEnabled", "IsSystem" },
                    values: new object[,]
                    {
                        { Id(0, Email, seed.Number), seed.Name + "_email", Email, true, false },
                        { Id(0, Sms, seed.Number), seed.Name + "_sms", Sms, true, false }
                    });

                migrationBuilder.InsertData(
                    table: "TemplateVersions",
                    columns: new[] { "Id", "Number", "Subject", "Body", "IsCurrent", "CreatedAt", "NotificationTemplateId" },
                    values: new object[,]
                    {
                        { Id(1, Email, seed.Number), 1, seed.Subject, seed.EmailBody, true, createdAt, Id(0, Email, seed.Number) },
                        { Id(1, Sms, seed.Number), 1, null, seed.SmsBody, true, createdAt, Id(0, Sms, seed.Number) }
                    });

                migrationBuilder.InsertData(
                    table: "NotificationTriggers",
                    columns: new[] { "Id", "EventKey", "Description", "IsEnabled" },
                    values: new object[] { Id(2, 0, seed.Number), seed.EventKey, seed.Description, true });

                migrationBuilder.InsertData(
                    table: "NotificationConfigurations",
                    columns: new[] { "Id", "TemplateId", "Channel", "IsEnabled", "NotificationTriggerId" },
                    values: new object[,]
                    {
                        { Id(3, Email, seed.Number), Id(0, Email, seed.Number), Email, true, Id(2, 0, seed.Number) },
                        { Id(3, Sms, seed.Number), Id(0, Sms, seed.Number), Sms, true, Id(2, 0, seed.Number) }
                    });
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            foreach (var seed in Seeds)
            {
                migrationBuilder.DeleteData(
                    table: "NotificationConfigurations",
                    keyColumn: "Id",
                    keyValues: new object[] { Id(3, Email, seed.Number), Id(3, Sms, seed.Number) });

                migrationBuilder.DeleteData(
                    table: "NotificationTriggers",
                    keyColumn: "Id",
                    keyValue: Id(2, 0, seed.Number));

                migrationBuilder.DeleteData(
                    table: "TemplateVersions",
                    keyColumn: "Id",
                    keyValues: new object[] { Id(1, Email, seed.Number), Id(1, Sms, seed.Number) });

                migrationBuilder.DeleteData(
                    table: "NotificationTemplates",
                    keyColumn: "Id",
                    keyValues: new object[] { Id(0, Email, seed.Number), Id(0, Sms, seed.Number) });
            }
        }

        private static Guid Id(int kind, int channel, int number)
            => new($"f2b4d5e3-000{kind}-4000-8000-0000000000{channel}{number}");

        private static string EmailBody(string heading, string intro, string highlight, string closing)
            => """
               <!DOCTYPE html>
               <html lang="es">
               <head>
                 <meta charset="utf-8" />
                 <meta name="viewport" content="width=device-width, initial-scale=1" />
                 <title>Llave</title>
               </head>
               <body style="margin:0; padding:0; background-color:#faf7f2;">
                 <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#faf7f2; padding:32px 16px;">
                   <tr>
                     <td align="center">
                       <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="max-width:540px; background-color:#ffffff; border:1px solid #e6dfd5; border-radius:14px; overflow:hidden;">
                         <tr>
                           <td style="background-color:#16302e; padding:28px 32px;">
                             <div style="font-family:Georgia,'Times New Roman',serif; font-size:24px; font-weight:700; color:#faf7f2; letter-spacing:0.01em;">Llave</div>
                             <div style="
               """ + Font + """
                font-size:12px; color:#b8c6c2; text-transform:uppercase; letter-spacing:0.12em; padding-top:6px;">Arriendo y venta de vivienda</div>
                           </td>
                         </tr>
                         <tr>
                           <td style="padding:36px 32px 8px 32px;
               """ + Font + """
                color:#1d2426;">
                             <p style="margin:0 0 6px 0; font-size:12px; font-weight:600; color:#b4532a; text-transform:uppercase; letter-spacing:0.12em;">Visitas Llave</p>
                             <h1 style="margin:0 0 12px 0; font-family:Georgia,'Times New Roman',serif; font-size:26px; font-weight:700; color:#16302e;">
               """ + heading + """
               </h1>
                             <p style="margin:0 0 8px 0; font-size:15px; line-height:1.6; color:#5b6669;">Hola, {{ recipient.name | html.escape }}:</p>
                             <p style="margin:0; font-size:15px; line-height:1.6; color:#5b6669;">
               """ + intro + """
               </p>
                           </td>
                         </tr>
                         <tr>
                           <td style="padding:20px 32px 0 32px;">
                             <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f7e8df; border:1px solid #edd5c6; border-radius:10px;">
                               <tr>
                                 <td style="padding:18px 20px;
               """ + Font + """
               ">
               """ + highlight + """

                                 </td>
                               </tr>
                             </table>
                           </td>
                         </tr>
                         <tr>
                           <td style="padding:16px 32px 0 32px;">
                             <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#faf7f2; border:1px solid #e6dfd5; border-radius:10px;">
                               <tr>
                                 <td style="padding:18px 20px;
               """ + Font + """
               ">
                                   <div style="font-size:12px; font-weight:600; color:#b4532a; text-transform:uppercase; letter-spacing:0.12em; padding-bottom:8px;">Inmueble</div>
                                   <div style="font-size:17px; font-weight:700; color:#16302e; line-height:1.4;">{{ listing_type }} en {{ listing_operation }} · {{ listing_neighborhood | html.escape }}, {{ listing_city | html.escape }}</div>
                                   <div style="font-size:14px; line-height:1.6; color:#5b6669; padding-top:4px;">{{ listing_address | html.escape }}</div>
                                   <div style="font-size:14px; line-height:1.6; color:#5b6669;">Precio publicado: <strong style="color:#1d2426;">{{ listing_price }}</strong></div>
                                 </td>
                               </tr>
                             </table>
                           </td>
                         </tr>
                         <tr>
                           <td style="padding:20px 32px 28px 32px;
               """ + Font + """
                font-size:14px; line-height:1.6; color:#5b6669;">
               """ + closing + """

                           </td>
                         </tr>
                         <tr>
                           <td style="padding:0 32px;"><div style="height:1px; background-color:#e6dfd5;"></div></td>
                         </tr>
                         <tr>
                           <td style="padding:20px 32px 28px 32px;
               """ + Font + """
                font-size:12px; line-height:1.6; color:#8a9295;">
                             Este es un mensaje automático de Llave, no respondas a este correo.<br />
                             © Llave · Colombia
                           </td>
                         </tr>
                       </table>
                     </td>
                   </tr>
                 </table>
               </body>
               </html>
               """;
    }
}
