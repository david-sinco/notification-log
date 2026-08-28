using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificationLog.NotificationService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "NotificationTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "varchar(100)", unicode: false, maxLength: 100, nullable: false),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    Subject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Body = table.Column<string>(type: "nvarchar(max)", maxLength: 20000, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTriggers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventKey = table.Column<string>(type: "varchar(200)", unicode: false, maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTriggers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NotificationConfigurations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    NotificationTriggerId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationConfigurations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationConfigurations_NotificationTemplates_TemplateId",
                        column: x => x.TemplateId,
                        principalTable: "NotificationTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_NotificationConfigurations_NotificationTriggers_NotificationTriggerId",
                        column: x => x.NotificationTriggerId,
                        principalTable: "NotificationTriggers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationConfigurations_TemplateId",
                table: "NotificationConfigurations",
                column: "TemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationConfigurations_Trigger_Channel",
                table: "NotificationConfigurations",
                columns: new[] { "NotificationTriggerId", "Channel" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_Channel_IsEnabled",
                table: "NotificationTemplates",
                columns: new[] { "Channel", "IsEnabled" });

            migrationBuilder.CreateIndex(
                name: "UX_NotificationTemplates_Name",
                table: "NotificationTemplates",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UX_NotificationTriggers_EventKey",
                table: "NotificationTriggers",
                column: "EventKey",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NotificationConfigurations");

            migrationBuilder.DropTable(
                name: "NotificationTemplates");

            migrationBuilder.DropTable(
                name: "NotificationTriggers");
        }
    }
}
