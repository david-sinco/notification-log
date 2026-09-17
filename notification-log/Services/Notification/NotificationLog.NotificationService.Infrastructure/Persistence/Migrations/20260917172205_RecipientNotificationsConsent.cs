using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NotificationLog.NotificationService.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class RecipientNotificationsConsent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AcceptsNotifications",
                table: "Recipients",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcceptsNotifications",
                table: "Recipients");
        }
    }
}
