using Microsoft.EntityFrameworkCore.Migrations;

namespace Profiles.API.Migrations
{
    public partial class updateNotificationTbl : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Error",
                table: "Notifications",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FirebaseResponse",
                table: "Notifications",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Success",
                table: "Notifications",
                type: "bit",
                nullable: false,
                defaultValue: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Error",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "FirebaseResponse",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "Success",
                table: "Notifications");
        }
    }
}
