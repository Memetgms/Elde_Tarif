using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elde_Tarif.Migrations
{
    /// <inheritdoc />
    public partial class AddTarifGoruntuleme : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TarifGoruntuleme",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AnonId = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    TarifId = table.Column<int>(type: "int", nullable: false),
                    ViewedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TarifGoruntuleme", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TarifGoruntuleme_AspNetUsers_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TarifGoruntuleme_Tarif_TarifId",
                        column: x => x.TarifId,
                        principalTable: "Tarif",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TarifGoruntuleme_AnonId_TarifId_ViewedAt",
                table: "TarifGoruntuleme",
                columns: new[] { "AnonId", "TarifId", "ViewedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TarifGoruntuleme_KullaniciId_TarifId_ViewedAt",
                table: "TarifGoruntuleme",
                columns: new[] { "KullaniciId", "TarifId", "ViewedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_TarifGoruntuleme_TarifId",
                table: "TarifGoruntuleme",
                column: "TarifId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TarifGoruntuleme");
        }
    }
}
