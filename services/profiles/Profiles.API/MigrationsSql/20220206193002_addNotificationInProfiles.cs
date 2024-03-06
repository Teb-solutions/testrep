using Microsoft.EntityFrameworkCore.Migrations;

namespace Profiles.API.Migrations
{
    public partial class addNotificationInProfiles : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "SendNotifications",
                table: "Profiles",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Branches",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SendNotifications",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Branches");
        }
    }
}
