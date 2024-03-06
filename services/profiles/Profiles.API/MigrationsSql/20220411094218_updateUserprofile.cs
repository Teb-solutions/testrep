using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Profiles.API.Migrations
{
    public partial class updateUserprofile : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.RenameColumn(
                name: "originLng",
                table: "Vehicles",
                newName: "OriginLng");

            migrationBuilder.AddColumn<string>(
                name: "GSTN",
                table: "BusinessEntities",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PAN",
                table: "BusinessEntities",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentNumber",
                table: "BusinessEntities",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UPIQRCodeImageUrl",
                table: "BusinessEntities",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GSTN",
                table: "BusinessEntities");

            migrationBuilder.DropColumn(
                name: "PAN",
                table: "BusinessEntities");

            migrationBuilder.DropColumn(
                name: "PaymentNumber",
                table: "BusinessEntities");

            migrationBuilder.DropColumn(
                name: "UPIQRCodeImageUrl",
                table: "BusinessEntities");

            migrationBuilder.RenameColumn(
                name: "OriginLng",
                table: "Vehicles",
                newName: "originLng");

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AutoAssignVehicle = table.Column<bool>(type: "bit", nullable: false),
                    AutoPlanManuallyAssigned = table.Column<bool>(type: "bit", nullable: false),
                    BranchId = table.Column<int>(type: "int", nullable: true),
                    BroadcastCheckVehLocUpdateThreshold = table.Column<bool>(type: "bit", nullable: false),
                    BroadcastCheckVehLoginState = table.Column<bool>(type: "bit", nullable: false),
                    BroadcastRadiusMet = table.Column<int>(type: "int", nullable: false),
                    BroadcastTakeCurrentVehLoc = table.Column<bool>(type: "bit", nullable: false),
                    BroadcastTimeOutBackendBufferSec = table.Column<int>(type: "int", nullable: false),
                    BroadcastTimeOutCustomerBufferSec = table.Column<int>(type: "int", nullable: false),
                    BroadcastTimeOutSec = table.Column<int>(type: "int", nullable: false),
                    BroadcastToDispatchedVeh = table.Column<bool>(type: "bit", nullable: false),
                    BroadcastVehLocIsGeoFenced = table.Column<bool>(type: "bit", nullable: false),
                    BroadcastVehLocUpdateThresholdSec = table.Column<int>(type: "int", nullable: false),
                    CheckVehLocIsGeoFencedForPlanning = table.Column<bool>(type: "bit", nullable: false),
                    CheckVehLocUpdateThreshold = table.Column<bool>(type: "bit", nullable: false),
                    CheckVehLoginStateForPlanning = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    GeoFenceOrderCreation = table.Column<bool>(type: "bit", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    ReAssignRejectedOrderThresholdSec = table.Column<int>(type: "int", nullable: true),
                    SlotThresholdLimit = table.Column<bool>(type: "bit", nullable: false),
                    TakeCurrentVehLocForPlanning = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<int>(type: "int", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedBy = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    VehLocMaxDistanceFromBranchMet = table.Column<int>(type: "int", nullable: false),
                    VehLocUpdateThresholdSec = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Settings_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Settings_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Settings_BranchId",
                table: "Settings",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Settings_TenantId",
                table: "Settings",
                column: "TenantId");
        }
    }
}
