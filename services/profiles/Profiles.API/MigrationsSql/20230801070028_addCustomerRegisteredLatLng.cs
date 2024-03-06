using Microsoft.EntityFrameworkCore.Migrations;

namespace Profiles.API.Migrations
{
    public partial class addCustomerRegisteredLatLng : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "RegisteredFromLat",
                table: "Profiles",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RegisteredFromLng",
                table: "Profiles",
                type: "float",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RegisteredFromLat",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "RegisteredFromLng",
                table: "Profiles");
        }
    }
}
