using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TutorPreClase.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class ExamenGeneradoPorIntento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "IntentoId",
                table: "pregunta",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PreguntasPorIntento",
                table: "examen",
                type: "integer",
                nullable: false,
                defaultValue: 6);

            migrationBuilder.CreateIndex(
                name: "IX_pregunta_IntentoId",
                table: "pregunta",
                column: "IntentoId");

            migrationBuilder.AddForeignKey(
                name: "FK_pregunta_intento_IntentoId",
                table: "pregunta",
                column: "IntentoId",
                principalTable: "intento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_pregunta_intento_IntentoId",
                table: "pregunta");

            migrationBuilder.DropIndex(
                name: "IX_pregunta_IntentoId",
                table: "pregunta");

            migrationBuilder.DropColumn(
                name: "IntentoId",
                table: "pregunta");

            migrationBuilder.DropColumn(
                name: "PreguntasPorIntento",
                table: "examen");
        }
    }
}
