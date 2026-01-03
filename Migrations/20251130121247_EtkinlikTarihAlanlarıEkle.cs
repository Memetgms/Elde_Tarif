using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Elde_Tarif.Migrations
{
    /// <inheritdoc />
    public partial class EtkinlikTarihAlanlarıEkle : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GunlukSular");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GunlukSular",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    KullaniciId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Ml = table.Column<int>(type: "int", nullable: false),
                    Tarih = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                name: "IX_GunlukSular_KullaniciId",
                table: "GunlukSular",
                column: "KullaniciId");
        }
    }
}
