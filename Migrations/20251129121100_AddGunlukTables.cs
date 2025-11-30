using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elde_Tarif.Migrations
{
    /// <inheritdoc />
    public partial class AddGunlukTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GunlukOgunler",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OgunTipi = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TarifId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GunlukOgunler", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GunlukOgunler_AspNetUsers_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_GunlukOgunler_Tarif_TarifId",
                        column: x => x.TarifId,
                        principalTable: "Tarif",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "GunlukSular",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Ml = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GunlukSular", x => x.Id);
                    table.ForeignKey(
                        name: "FK_GunlukSular_AspNetUsers_KullaniciId",
                        column: x => x.KullaniciId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GunlukOgunler_KullaniciId",
                table: "GunlukOgunler",
                column: "KullaniciId");

            migrationBuilder.CreateIndex(
                name: "IX_GunlukOgunler_TarifId",
                table: "GunlukOgunler",
                column: "TarifId");

            migrationBuilder.CreateIndex(
                name: "IX_GunlukSular_KullaniciId",
                table: "GunlukSular",
                column: "KullaniciId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GunlukOgunler");

            migrationBuilder.DropTable(
                name: "GunlukSular");
        }
    }
}
