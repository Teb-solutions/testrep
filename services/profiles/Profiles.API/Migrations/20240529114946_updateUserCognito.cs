using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Profiles.API.Migrations
{
    /// <inheritdoc />
    public partial class updateUserCognito : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "Users",
                type: "timestamp without time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovedBy",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CognitoUsername",
                table: "Users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Users",
                type: "boolean",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Users_ApprovedBy",
                table: "Users",
                column: "ApprovedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Users_CognitoUsername",
                table: "Users",
                column: "CognitoUsername");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Users_ApprovedBy",
                table: "Users",
                column: "ApprovedBy",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Users_ApprovedBy",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_ApprovedBy",
                table: "Users");

            migrationBuilder.DropIndex(
                name: "IX_Users_CognitoUsername",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ApprovedBy",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "CognitoUsername",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Users");
        }
    }
}
