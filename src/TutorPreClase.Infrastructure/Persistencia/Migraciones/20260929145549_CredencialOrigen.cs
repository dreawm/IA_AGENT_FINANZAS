using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TutorPreClase.Infrastructure.Persistencia.Migraciones
{
    /// <summary>
    /// Origen de la credencial: pegada por el alumno u obtenida por OAuth (RF-29). Las
    /// existentes quedan como "Pegada", que es como se conectaron.
    /// </summary>
    public partial class CredencialOrigen : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Origen",
                table: "credencial_agente",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Pegada");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Origen",
                table: "credencial_agente");
        }
    }
}
