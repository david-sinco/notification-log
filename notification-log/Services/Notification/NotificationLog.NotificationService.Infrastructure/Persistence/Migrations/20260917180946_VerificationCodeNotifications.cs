using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificationLog.NotificationService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class VerificationCodeNotifications : Migration
    {
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
                        "Confirma tu cuenta",
                        "Hola,\n\nTu código de verificación es {{ code }}.\n\nVence en pocos minutos. Si no solicitaste este código, ignora este mensaje.",
                        true,
                        new DateTime(2026, 9, 17, 0, 0, 0, DateTimeKind.Utc),
                        new Guid("f1a3c4d2-0000-4000-8000-000000000001")
                    },
                    {
                        new Guid("f1a3c4d2-0001-4000-8000-000000000002"),
                        1,
                        null,
                        "Tu código de verificación es {{ code }}. Vence en pocos minutos.",
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
