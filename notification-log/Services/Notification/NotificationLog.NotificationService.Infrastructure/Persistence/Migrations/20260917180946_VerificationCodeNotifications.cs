using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificationLog.NotificationService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class VerificationCodeNotifications : Migration
    {
        private const string VerificationCodeSmsBody =
            "Llave: tu código de verificación es {{ code }}. Vence en pocos minutos. No lo compartas con nadie.";

        private const string VerificationCodeEmailBody =
            """
            <!DOCTYPE html>
            <html lang="es">
            <head>
              <meta charset="utf-8" />
              <meta name="viewport" content="width=device-width, initial-scale=1" />
              <title>Tu código de verificación</title>
            </head>
            <body style="margin:0; padding:0; background-color:#faf7f2;">
              <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#faf7f2; padding:32px 16px;">
                <tr>
                  <td align="center">
                    <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="max-width:540px; background-color:#ffffff; border:1px solid #e6dfd5; border-radius:14px; overflow:hidden;">
                      <tr>
                        <td style="background-color:#16302e; padding:28px 32px;">
                          <div style="font-family:Georgia,'Times New Roman',serif; font-size:24px; font-weight:700; color:#faf7f2; letter-spacing:0.01em;">Llave</div>
                          <div style="font-family:'Segoe UI',Roboto,Helvetica,Arial,sans-serif; font-size:12px; color:#b8c6c2; text-transform:uppercase; letter-spacing:0.12em; padding-top:6px;">Arriendo y venta de vivienda</div>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:36px 32px 8px 32px; font-family:'Segoe UI',Roboto,Helvetica,Arial,sans-serif; color:#1d2426;">
                          <p style="margin:0 0 6px 0; font-size:12px; font-weight:600; color:#b4532a; text-transform:uppercase; letter-spacing:0.12em;">Cuentas Llave</p>
                          <h1 style="margin:0 0 12px 0; font-family:Georgia,'Times New Roman',serif; font-size:26px; font-weight:700; color:#16302e;">Confirma tu cuenta</h1>
                          <p style="margin:0; font-size:15px; line-height:1.6; color:#5b6669;">Usa este código para terminar de crear tu cuenta. Escríbelo en la pantalla de verificación que dejaste abierta.</p>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:28px 32px;">
                          <table role="presentation" width="100%" cellpadding="0" cellspacing="0" border="0" style="background-color:#f7e8df; border:1px solid #edd5c6; border-radius:10px;">
                            <tr>
                              <td align="center" style="padding:22px 16px; font-family:'Segoe UI',Roboto,Helvetica,Arial,sans-serif; font-size:32px; font-weight:700; letter-spacing:0.28em; color:#8f3f1f;">{{ code }}</td>
                            </tr>
                          </table>
                          <p style="margin:16px 0 0 0; font-family:'Segoe UI',Roboto,Helvetica,Arial,sans-serif; font-size:14px; line-height:1.6; color:#5b6669;">El código vence en pocos minutos. Por seguridad, no lo compartas con nadie: el equipo de Llave nunca te lo pedirá por teléfono ni por chat.</p>
                          <p style="margin:12px 0 0 0; font-family:'Segoe UI',Roboto,Helvetica,Arial,sans-serif; font-size:14px; line-height:1.6; color:#5b6669;">Si no solicitaste este código, ignora este mensaje y tu cuenta seguirá igual.</p>
                        </td>
                      </tr>
                      <tr>
                        <td style="padding:0 32px;"><div style="height:1px; background-color:#e6dfd5;"></div></td>
                      </tr>
                      <tr>
                        <td style="padding:20px 32px 28px 32px; font-family:'Segoe UI',Roboto,Helvetica,Arial,sans-serif; font-size:12px; line-height:1.6; color:#8a9295;">
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

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSystem",
                table: "NotificationTemplates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<Guid>(
                name: "RecipientId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "Payload",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<Guid>(
                name: "ConfigurationId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.InsertData(
                table: "NotificationTemplates",
                columns: new[] { "Id", "Name", "Channel", "IsEnabled", "IsSystem" },
                values: new object[,]
                {
                    { new Guid("f1a3c4d2-0000-4000-8000-000000000001"), "verificacion_codigo_email", 1, true, true },
                    { new Guid("f1a3c4d2-0000-4000-8000-000000000002"), "verificacion_codigo_sms", 2, true, true }
                });

            migrationBuilder.InsertData(
                table: "TemplateVersions",
                columns: new[] { "Id", "Number", "Subject", "Body", "IsCurrent", "CreatedAt", "NotificationTemplateId" },
                values: new object[,]
                {
                    {
                        new Guid("f1a3c4d2-0001-4000-8000-000000000001"),
                        1,
                        "Tu código de verificación · Llave",
                        VerificationCodeEmailBody,
                        true,
                        new DateTime(2026, 9, 17, 0, 0, 0, DateTimeKind.Utc),
                        new Guid("f1a3c4d2-0000-4000-8000-000000000001")
                    },
                    {
                        new Guid("f1a3c4d2-0001-4000-8000-000000000002"),
                        1,
                        null,
                        VerificationCodeSmsBody,
                        true,
                        new DateTime(2026, 9, 17, 0, 0, 0, DateTimeKind.Utc),
                        new Guid("f1a3c4d2-0000-4000-8000-000000000002")
                    }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "TemplateVersions",
                keyColumn: "Id",
                keyValues: new object[]
                {
                    new Guid("f1a3c4d2-0001-4000-8000-000000000001"),
                    new Guid("f1a3c4d2-0001-4000-8000-000000000002")
                });

            migrationBuilder.DeleteData(
                table: "NotificationTemplates",
                keyColumn: "Id",
                keyValues: new object[]
                {
                    new Guid("f1a3c4d2-0000-4000-8000-000000000001"),
                    new Guid("f1a3c4d2-0000-4000-8000-000000000002")
                });

            migrationBuilder.DropColumn(
                name: "IsSystem",
                table: "NotificationTemplates");

            migrationBuilder.AlterColumn<Guid>(
                name: "RecipientId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Payload",
                table: "Notifications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "ConfigurationId",
                table: "Notifications",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }
    }
}
