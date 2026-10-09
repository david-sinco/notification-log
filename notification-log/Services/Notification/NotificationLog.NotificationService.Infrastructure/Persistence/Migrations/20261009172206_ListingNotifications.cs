using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificationLog.NotificationService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ListingNotifications : Migration
    {
        private const int Email = 1;
        private const int Sms = 2;

        private const string Font = "font-family:'Segoe UI',Roboto,Helvetica,Arial,sans-serif;";

        private const string SmsListing = "{{ listing_title }}";

        private const string ExpiresHighlight =
            """
            <div style="font-size:12px; font-weight:600; color:#8f3f1f; text-transform:uppercase; letter-spacing:0.12em; padding-bottom:8px;">Publicada hasta · hora de Colombia</div>
            <div style="font-size:22px; font-weight:700; color:#8f3f1f;">{{ expires_at }}</div>
            """;

        private const string PriceHighlight =
            """
            <div style="font-size:12px; font-weight:600; color:#8f3f1f; text-transform:uppercase; letter-spacing:0.12em; padding-bottom:8px;">Cambio de precio</div>
            <div style="font-size:15px; line-height:1.6; color:#5b6669; text-decoration:line-through;">{{ old_price }}</div>
            <div style="font-size:22px; font-weight:700; color:#8f3f1f;">{{ new_price }}</div>
            """;

        private const string ReasonsHighlight =
            """
            <div style="font-size:12px; font-weight:600; color:#8f3f1f; text-transform:uppercase; letter-spacing:0.12em; padding-bottom:8px;">Motivos del rechazo</div>
            {{ for item in reasons | string.split ", " }}<div style="font-size:15px; line-height:1.6; color:#1d2426;">· {{ item | html.escape }}</div>{{ end }}
            """;

        private const string ReasonHighlight =
            """
            <div style="font-size:12px; font-weight:600; color:#8f3f1f; text-transform:uppercase; letter-spacing:0.12em; padding-bottom:8px;">Motivo</div>
            <div style="font-size:15px; line-height:1.6; color:#1d2426;">{{ reason | html.escape }}</div>
            """;

        private const string ClosedHighlight =
            """
            <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0">
              <tr>
                <td style="width:50%; vertical-align:top; font-family:'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
                  <div style="font-size:12px; font-weight:600; color:#8f3f1f; text-transform:uppercase; letter-spacing:0.12em; padding-bottom:8px;">Precio final</div>
                  <div style="font-size:20px; font-weight:700; color:#8f3f1f;">{{ final_price }}</div>
                </td>
                <td style="width:50%; vertical-align:top; font-family:'Segoe UI',Roboto,Helvetica,Arial,sans-serif;">
                  <div style="font-size:12px; font-weight:600; color:#8f3f1f; text-transform:uppercase; letter-spacing:0.12em; padding-bottom:8px;">Fecha de firma</div>
                  <div style="font-size:20px; font-weight:700; color:#8f3f1f;">{{ signed_on }}</div>
                </td>
              </tr>
            </table>
            """;

        private static readonly (int Number, string EventKey, string Name, string Description, string Subject, string EmailBody, string SmsBody)[] Seeds =
        {
            (
                1,
                "publicacion.creada",
                "publicacion_creada",
                "El propietario o un moderador crea una publicación en borrador; se avisa al anfitrión.",
                "Se creó tu publicación · {{ listing_title }}",
                EmailBody(
                    "Tu publicación está en borrador",
                    "Se creó una publicación para tu inmueble en Llave. Por ahora es un borrador, así que nadie más puede verla.",
                    StatusHighlight("Borrador", "Completa los datos del inmueble, la ubicación, el precio y las fotos."),
                    "Cuando esté completa, envíala a revisión desde Llave. Un moderador la revisará antes de publicarla."),
                "Llave: se creó tu publicación " + SmsListing + " como borrador. Complétala y envíala a revisión desde Llave."
            ),
            (
                2,
                "publicacion.datos.actualizados",
                "publicacion_datos_actualizados",
                "Se actualizan los datos del inmueble, la ubicación o la descripción de una publicación; se avisa al anfitrión.",
                "Se actualizaron los datos de tu publicación · {{ listing_title }}",
                EmailBody(
                    "Se actualizaron los datos de tu publicación",
                    "Se guardaron cambios en los datos del inmueble, la ubicación o la descripción de esta publicación.",
                    NoticeHighlight("Cambios guardados", "Datos del inmueble", "Si estaba publicada o pausada, vuelve a revisión antes de mostrarse otra vez."),
                    "Revisa en Llave que la información esté correcta. Si la publicación volvió a revisión, te avisaremos cuando un moderador la revise."),
                "Llave: se actualizaron los datos de tu publicación " + SmsListing + ". Si estaba publicada o pausada, vuelve a revisión."
            ),
            (
                3,
                "publicacion.fotos.actualizadas",
                "publicacion_fotos_actualizadas",
                "Se agregan, quitan o reordenan las fotos de una publicación; se avisa al anfitrión.",
                "Se actualizaron las fotos de tu publicación · {{ listing_title }}",
                EmailBody(
                    "Se actualizaron las fotos de tu publicación",
                    "Se agregaron, quitaron o reordenaron las fotos de esta publicación.",
                    NoticeHighlight("Fotos de la publicación", "{{ photo_count }} en total", "Si estaba publicada o pausada, vuelve a revisión antes de mostrarse otra vez."),
                    "Las fotos son lo primero que ven los visitantes: revisa en Llave que se vean bien y en el orden que quieres."),
                "Llave: se actualizaron las fotos de tu publicación " + SmsListing + " ({{ photo_count }} en total). Si estaba publicada o pausada, vuelve a revisión."
            ),
            (
                4,
                "publicacion.precio.cambiado",
                "publicacion_precio_cambiado",
                "Cambia el precio de una publicación; se avisa al anfitrión.",
                "Nuevo precio para tu publicación · {{ listing_title }}",
                EmailBody(
                    "Cambió el precio de tu publicación",
                    "Se guardó un nuevo precio para esta publicación.",
                    PriceHighlight,
                    "Mientras la publicación esté publicada, el precio solo se puede cambiar una vez cada 24 horas."),
                "Llave: el precio de tu publicación " + SmsListing + " cambió de {{ old_price }} a {{ new_price }}."
            ),
            (
                5,
                "publicacion.en.revision",
                "publicacion_en_revision",
                "Una publicación se envía a revisión de un moderador; se avisa al anfitrión.",
                "Tu publicación está en revisión · {{ listing_title }}",
                EmailBody(
                    "Tu publicación está en revisión",
                    "Recibimos tu publicación y quedó en la fila de revisión de nuestros moderadores.",
                    StatusHighlight("En revisión", "Un moderador revisará los datos, el precio y las fotos antes de publicarla."),
                    "Te avisaremos cuando la aprueben o si hay algo por corregir."),
                "Llave: tu publicación " + SmsListing + " está en revisión. Te avisaremos cuando un moderador la revise."
            ),
            (
                6,
                "publicacion.aprobada",
                "publicacion_aprobada",
                "Un moderador aprueba una publicación y queda publicada; se avisa al anfitrión.",
                "Tu publicación fue aprobada · {{ listing_title }}",
                EmailBody(
                    "Tu publicación ya está en Llave",
                    "Un moderador aprobó tu publicación y ya está visible para los visitantes.",
                    ExpiresHighlight,
                    "Desde ahora los visitantes pueden pedir visitas a tu inmueble. Antes de que venza podrás renovarla desde Llave."),
                "Llave: tu publicación " + SmsListing + " fue aprobada y ya está publicada hasta el {{ expires_at }}."
            ),
            (
                7,
                "publicacion.rechazada",
                "publicacion_rechazada",
                "Un moderador rechaza una publicación y vuelve a borrador; se avisa al anfitrión.",
                "Tu publicación necesita cambios · {{ listing_title }}",
                EmailBody(
                    "Tu publicación necesita cambios",
                    "Un moderador revisó tu publicación y no la pudo aprobar por estos motivos. La publicación volvió a borrador.",
                    ReasonsHighlight,
                    "Corrige estos puntos y vuelve a enviarla a revisión desde Llave."),
                "Llave: tu publicación " + SmsListing + " fue rechazada. Motivos: {{ reasons }}. Corrígela y envíala de nuevo a revisión."
            ),
            (
                8,
                "publicacion.pausada",
                "publicacion_pausada",
                "Se pausa una publicación: sigue visible pero no recibe visitas nuevas; se avisa al anfitrión.",
                "Tu publicación está pausada · {{ listing_title }}",
                EmailBody(
                    "Tu publicación está pausada",
                    "Esta publicación quedó en pausa.",
                    StatusHighlight("Pausada", "Sigue visible en Llave, pero no recibe solicitudes de visita nuevas."),
                    "Reanúdala desde Llave cuando quieras volver a recibir visitas."),
                "Llave: tu publicación " + SmsListing + " está pausada. Sigue visible, pero no recibe visitas nuevas."
            ),
            (
                9,
                "publicacion.reanudada",
                "publicacion_reanudada",
                "Se reanuda una publicación pausada; se avisa al anfitrión.",
                "Tu publicación se reanudó · {{ listing_title }}",
                EmailBody(
                    "Tu publicación se reanudó",
                    "Esta publicación salió de la pausa y está publicada de nuevo.",
                    StatusHighlight("Publicada", "Los visitantes pueden volver a pedir visitas."),
                    "Responde a tiempo las solicitudes de visita que lleguen desde Llave."),
                "Llave: tu publicación " + SmsListing + " se reanudó y ya recibe visitas de nuevo."
            ),
            (
                10,
                "publicacion.renovada",
                "publicacion_renovada",
                "Se renueva la vigencia de una publicación; se avisa al anfitrión.",
                "Tu publicación fue renovada · {{ listing_title }}",
                EmailBody(
                    "Tu publicación fue renovada",
                    "Se renovó la vigencia de esta publicación, así que seguirá visible para los visitantes.",
                    ExpiresHighlight,
                    "Antes de que venza podrás renovarla otra vez desde Llave."),
                "Llave: tu publicación " + SmsListing + " fue renovada y sigue publicada hasta el {{ expires_at }}."
            ),
            (
                11,
                "publicacion.vencida",
                "publicacion_vencida",
                "Se vence la vigencia de una publicación; se avisa al anfitrión.",
                "Tu publicación venció · {{ listing_title }}",
                EmailBody(
                    "Tu publicación venció",
                    "Terminó la vigencia de esta publicación.",
                    StatusHighlight("Vencida", "Ya no recibe solicitudes de visita nuevas."),
                    "Si el inmueble sigue disponible, renueva la publicación desde Llave para volver a recibir visitas."),
                "Llave: tu publicación " + SmsListing + " venció. Si el inmueble sigue disponible, renuévala desde Llave."
            ),
            (
                12,
                "publicacion.cerrada",
                "publicacion_cerrada",
                "Se cierra una publicación porque el inmueble se arrendó o se vendió; se avisa al anfitrión.",
                "Cerraste el negocio · {{ listing_title }}",
                EmailBody(
                    "¡Felicitaciones, cerraste el negocio!",
                    "Tu publicación se cerró porque el inmueble se arrendó o se vendió. Ya no está disponible en Llave.",
                    ClosedHighlight,
                    "Gracias por usar Llave. Si vuelves a tener un inmueble disponible, puedes publicarlo cuando quieras."),
                "Llave: tu publicación " + SmsListing + " se cerró por {{ final_price }}, firmado el {{ signed_on }}. ¡Felicitaciones!"
            ),
            (
                13,
                "publicacion.retirada",
                "publicacion_retirada",
                "Se retira una publicación del sitio; se avisa al anfitrión.",
                "Tu publicación fue retirada · {{ listing_title }}",
                EmailBody(
                    "Tu publicación fue retirada",
                    "Esta publicación se retiró de Llave y ya no se muestra a los visitantes.",
                    ReasonHighlight,
                    "Si quieres volver a ofrecer el inmueble, puedes crear una nueva publicación desde Llave."),
                "Llave: tu publicación " + SmsListing + " fue retirada. Motivo: {{ reason }}"
            ),
            (
                14,
                "publicacion.suspendida",
                "publicacion_suspendida",
                "Un moderador suspende una publicación; se avisa al anfitrión.",
                "Tu publicación fue suspendida · {{ listing_title }}",
                EmailBody(
                    "Tu publicación fue suspendida",
                    "Un moderador suspendió esta publicación. Mientras siga suspendida no se muestra a los visitantes ni recibe solicitudes de visita.",
                    ReasonHighlight,
                    "Revisa el motivo y corrige lo necesario. Te avisaremos cuando un moderador levante la suspensión."),
                "Llave: un moderador suspendió tu publicación " + SmsListing + ". Motivo: {{ reason }}"
            ),
            (
                15,
                "publicacion.rehabilitada",
                "publicacion_rehabilitada",
                "Un moderador levanta la suspensión de una publicación y vuelve a publicada; se avisa al anfitrión.",
                "Tu publicación está activa de nuevo · {{ listing_title }}",
                EmailBody(
                    "Tu publicación está activa de nuevo",
                    "Un moderador levantó la suspensión de esta publicación.",
                    StatusHighlight("Publicada", "Los visitantes pueden volver a verla y pedir visitas."),
                    "Gracias por atender las indicaciones de nuestros moderadores."),
                "Llave: un moderador levantó la suspensión de tu publicación " + SmsListing + ". Ya está publicada de nuevo."
            )
        };

        private static string StatusHighlight(string status, string detail)
            => NoticeHighlight("Estado de la publicación", status, detail);

        private static string NoticeHighlight(string label, string title, string detail)
            => """
               <div style="font-size:12px; font-weight:600; color:#8f3f1f; text-transform:uppercase; letter-spacing:0.12em; padding-bottom:8px;">
               """ + label + """
               </div>
               <div style="font-size:22px; font-weight:700; color:#8f3f1f;">
               """ + title + """
               </div>
               <div style="font-size:14px; line-height:1.6; color:#1d2426; padding-top:6px;">
               """ + detail + """
               </div>
               """;

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var createdAt = new DateTime(2026, 10, 9, 0, 0, 0, DateTimeKind.Utc);

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
            => new($"f2b4d5e4-000{kind}-4000-8000-000000000{channel}{number:D2}");

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
                             <p style="margin:0 0 6px 0; font-size:12px; font-weight:600; color:#b4532a; text-transform:uppercase; letter-spacing:0.12em;">Publicaciones Llave</p>
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
                                   <div style="font-size:17px; font-weight:700; color:#16302e; line-height:1.4;">{{ listing_title | html.escape }}</div>
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
