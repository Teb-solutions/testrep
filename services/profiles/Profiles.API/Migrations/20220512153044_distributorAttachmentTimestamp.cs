using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Profiles.API.Migrations
{
    public partial class distributorAttachmentTimestamp : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "BusinessEntityAttachedAt",
                table: "Users",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BusinessEntityAttachedByUserId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_BusinessEntityAttachedByUserId",
                table: "Users",
                column: "BusinessEntityAttachedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_BusinessEntityAttachedByUserId",
                table: "Users",
                column: "BusinessEntityAttachedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_BusinessEntityAttachedByUserId",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_BusinessEntityAttachedByUserId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BusinessEntityAttachedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "BusinessEntityAttachedByUserId",
                table: "Users");
        }
    }
}
