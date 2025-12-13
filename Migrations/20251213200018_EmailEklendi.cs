using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elde_Tarif.Migrations
{
    /// <inheritdoc />
    public partial class EmailEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EmailActivationCode",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailActivationExpiresAt",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EmailActivationTryCount",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailActivationCode",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailActivationExpiresAt",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailActivationTryCount",
                table: "AspNetUsers");
        }
    }
}
