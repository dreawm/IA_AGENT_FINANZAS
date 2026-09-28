using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TutorPreClase.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class CredencialesByok : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Rol",
                table: "usuario",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "usuario",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "usuario",
                type: "TEXT",
                maxLength: 320,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(320)",
                oldMaxLength: 320);

            migrationBuilder.AlterColumn<string>(
                name: "AgentePreferido",
                table: "usuario",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "usuario",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "TextoOriginalAlumno",
                table: "respuesta_intento",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<Guid>(
                name: "PreguntaId",
                table: "respuesta_intento",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "IntentoId",
                table: "respuesta_intento",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<bool>(
                name: "EsCorrecta",
                table: "respuesta_intento",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<long>(
                name: "CreadoEn",
                table: "respuesta_intento",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Guid>(
                name: "AlternativaId",
                table: "respuesta_intento",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "respuesta_intento",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "PaginaId",
                table: "pregunta_referencia",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "PreguntaId",
                table: "pregunta_referencia",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Tema",
                table: "pregunta",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Origen",
                table: "pregunta",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "Orden",
                table: "pregunta",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Nivel",
                table: "pregunta",
                type: "TEXT",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Justificacion",
                table: "pregunta",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<Guid>(
                name: "ExamenId",
                table: "pregunta",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Enunciado",
                table: "pregunta",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<bool>(
                name: "Aprobada",
                table: "pregunta",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "pregunta",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<int>(
                name: "Tokens",
                table: "pagina_contenido",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Texto",
                table: "pagina_contenido",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<int>(
                name: "Pagina",
                table: "pagina_contenido",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClaseId",
                table: "pagina_contenido",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "ArchivoId",
                table: "pagina_contenido",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "pagina_contenido",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "TemasDebiles",
                table: "nivel_alumno",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "Origen",
                table: "nivel_alumno",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Nivel",
                table: "nivel_alumno",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<Guid>(
                name: "ClaseId",
                table: "nivel_alumno",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "AlumnoId",
                table: "nivel_alumno",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<long>(
                name: "ActualizadoEn",
                table: "nivel_alumno",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "nivel_alumno",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<bool>(
                name: "UsaAmpliacion",
                table: "mensaje_chat",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<int>(
                name: "TokensSalida",
                table: "mensaje_chat",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<int>(
                name: "TokensEntrada",
                table: "mensaje_chat",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "Texto",
                table: "mensaje_chat",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Rol",
                table: "mensaje_chat",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<Guid>(
                name: "PreguntaId",
                table: "mensaje_chat",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Herramienta",
                table: "mensaje_chat",
                type: "TEXT",
                maxLength: 60,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(60)",
                oldMaxLength: 60,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Fuentes",
                table: "mensaje_chat",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "CreadoEn",
                table: "mensaje_chat",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Guid>(
                name: "ConversacionId",
                table: "mensaje_chat",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "AgenteId",
                table: "mensaje_chat",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "mensaje_chat",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "RolEnCurso",
                table: "matricula",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<Guid>(
                name: "CursoId",
                table: "matricula",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "UsuarioId",
                table: "matricula",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<decimal>(
                name: "Puntaje",
                table: "intento",
                type: "TEXT",
                precision: 4,
                scale: 1,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(4,1)",
                oldPrecision: 4,
                oldScale: 1,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Porcentaje",
                table: "intento",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Modelo",
                table: "intento",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<long>(
                name: "Inicio",
                table: "intento",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Guid>(
                name: "ExamenId",
                table: "intento",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "intento",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<long>(
                name: "Envio",
                table: "intento",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "AlumnoId",
                table: "intento",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "AgenteId",
                table: "intento",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "intento",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<bool>(
                name: "Publicado",
                table: "examen",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "ModoFeedback",
                table: "examen",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "MinutosLimite",
                table: "examen",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MaxIntentos",
                table: "examen",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClaseId",
                table: "examen",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<long>(
                name: "CierraEn",
                table: "examen",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<long>(
                name: "AbreEn",
                table: "examen",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "examen",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Texto",
                table: "duda_sin_cobertura",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Tema",
                table: "duda_sin_cobertura",
                type: "TEXT",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "RespondidaConAmpliacion",
                table: "duda_sin_cobertura",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<long>(
                name: "CreadoEn",
                table: "duda_sin_cobertura",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Guid>(
                name: "ConversacionId",
                table: "duda_sin_cobertura",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClaseId",
                table: "duda_sin_cobertura",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "duda_sin_cobertura",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Periodo",
                table: "curso",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "curso",
                type: "TEXT",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "curso",
                type: "TEXT",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "curso",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Modo",
                table: "conversacion",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<Guid>(
                name: "IntentoId",
                table: "conversacion",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "CreadaEn",
                table: "conversacion",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClaseId",
                table: "conversacion",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<Guid>(
                name: "AlumnoId",
                table: "conversacion",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "AgenteId",
                table: "conversacion",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "conversacion",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Titulo",
                table: "clase",
                type: "TEXT",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<int>(
                name: "Orden",
                table: "clase",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<long>(
                name: "Inicio",
                table: "clase",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Guid>(
                name: "CursoId",
                table: "clase",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<bool>(
                name: "AmpliacionPermitida",
                table: "clase",
                type: "INTEGER",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "clase",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Tipo",
                table: "archivo_contenido",
                type: "TEXT",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "archivo_contenido",
                type: "TEXT",
                maxLength: 400,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(400)",
                oldMaxLength: 400);

            migrationBuilder.AlterColumn<string>(
                name: "HashSha256",
                table: "archivo_contenido",
                type: "TEXT",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(64)",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "archivo_contenido",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Error",
                table: "archivo_contenido",
                type: "TEXT",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<long>(
                name: "CreadoEn",
                table: "archivo_contenido",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClaseId",
                table: "archivo_contenido",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "BlobUrl",
                table: "archivo_contenido",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "archivo_contenido",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Texto",
                table: "alternativa",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<Guid>(
                name: "PreguntaId",
                table: "alternativa",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Letra",
                table: "alternativa",
                type: "TEXT",
                maxLength: 1,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1)",
                oldMaxLength: 1);

            migrationBuilder.AlterColumn<bool>(
                name: "EsCorrecta",
                table: "alternativa",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "alternativa",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<string>(
                name: "Proveedor",
                table: "agente_ia",
                type: "TEXT",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "NombreVisible",
                table: "agente_ia",
                type: "TEXT",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(60)",
                oldMaxLength: 60);

            migrationBuilder.AlterColumn<string>(
                name: "Modelo",
                table: "agente_ia",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "Habilitado",
                table: "agente_ia",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "agente_ia",
                type: "TEXT",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BaseUrl",
                table: "agente_ia",
                type: "TEXT",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(300)",
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "agente_ia",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "UrlConsola",
                table: "agente_ia",
                type: "TEXT",
                maxLength: 300,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "credencial_agente",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    UsuarioId = table.Column<Guid>(type: "TEXT", nullable: false),
                    AgenteId = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    ClaveCifrada = table.Column<string>(type: "TEXT", nullable: false),
                    Ultimos4 = table.Column<string>(type: "TEXT", maxLength: 4, nullable: false),
                    Estado = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    CreadaEn = table.Column<long>(type: "INTEGER", nullable: false),
                    UltimoUsoEn = table.Column<long>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_credencial_agente", x => x.Id);
                    table.ForeignKey(
                        name: "FK_credencial_agente_usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_credencial_agente_UsuarioId_AgenteId",
                table: "credencial_agente",
                columns: new[] { "UsuarioId", "AgenteId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "credencial_agente");

            migrationBuilder.DropColumn(
                name: "UrlConsola",
                table: "agente_ia");

            migrationBuilder.AlterColumn<string>(
                name: "Rol",
                table: "usuario",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "usuario",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "usuario",
                type: "character varying(320)",
                maxLength: 320,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 320);

            migrationBuilder.AlterColumn<string>(
                name: "AgentePreferido",
                table: "usuario",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "usuario",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "TextoOriginalAlumno",
                table: "respuesta_intento",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "PreguntaId",
                table: "respuesta_intento",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "IntentoId",
                table: "respuesta_intento",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<bool>(
                name: "EsCorrecta",
                table: "respuesta_intento",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreadoEn",
                table: "respuesta_intento",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<Guid>(
                name: "AlternativaId",
                table: "respuesta_intento",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "respuesta_intento",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "PaginaId",
                table: "pregunta_referencia",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "PreguntaId",
                table: "pregunta_referencia",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Tema",
                table: "pregunta",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Origen",
                table: "pregunta",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "Orden",
                table: "pregunta",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "Nivel",
                table: "pregunta",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Justificacion",
                table: "pregunta",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "ExamenId",
                table: "pregunta",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Enunciado",
                table: "pregunta",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<bool>(
                name: "Aprobada",
                table: "pregunta",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "pregunta",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "Tokens",
                table: "pagina_contenido",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "Texto",
                table: "pagina_contenido",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<int>(
                name: "Pagina",
                table: "pagina_contenido",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClaseId",
                table: "pagina_contenido",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "ArchivoId",
                table: "pagina_contenido",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "pagina_contenido",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "TemasDebiles",
                table: "nivel_alumno",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Origen",
                table: "nivel_alumno",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Nivel",
                table: "nivel_alumno",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<Guid>(
                name: "ClaseId",
                table: "nivel_alumno",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "AlumnoId",
                table: "nivel_alumno",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "ActualizadoEn",
                table: "nivel_alumno",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "nivel_alumno",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<bool>(
                name: "UsaAmpliacion",
                table: "mensaje_chat",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "TokensSalida",
                table: "mensaje_chat",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<int>(
                name: "TokensEntrada",
                table: "mensaje_chat",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "Texto",
                table: "mensaje_chat",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Rol",
                table: "mensaje_chat",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<Guid>(
                name: "PreguntaId",
                table: "mensaje_chat",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Herramienta",
                table: "mensaje_chat",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 60,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Fuentes",
                table: "mensaje_chat",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreadoEn",
                table: "mensaje_chat",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<Guid>(
                name: "ConversacionId",
                table: "mensaje_chat",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "AgenteId",
                table: "mensaje_chat",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "mensaje_chat",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "RolEnCurso",
                table: "matricula",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<Guid>(
                name: "CursoId",
                table: "matricula",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "UsuarioId",
                table: "matricula",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<decimal>(
                name: "Puntaje",
                table: "intento",
                type: "numeric(4,1)",
                precision: 4,
                scale: 1,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "TEXT",
                oldPrecision: 4,
                oldScale: 1,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Porcentaje",
                table: "intento",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Modelo",
                table: "intento",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Inicio",
                table: "intento",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<Guid>(
                name: "ExamenId",
                table: "intento",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "intento",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Envio",
                table: "intento",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(long),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "AlumnoId",
                table: "intento",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "AgenteId",
                table: "intento",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "intento",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<bool>(
                name: "Publicado",
                table: "examen",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "ModoFeedback",
                table: "examen",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<int>(
                name: "MinutosLimite",
                table: "examen",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "MaxIntentos",
                table: "examen",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClaseId",
                table: "examen",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CierraEn",
                table: "examen",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "AbreEn",
                table: "examen",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "examen",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Texto",
                table: "duda_sin_cobertura",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Tema",
                table: "duda_sin_cobertura",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<bool>(
                name: "RespondidaConAmpliacion",
                table: "duda_sin_cobertura",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreadoEn",
                table: "duda_sin_cobertura",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<Guid>(
                name: "ConversacionId",
                table: "duda_sin_cobertura",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClaseId",
                table: "duda_sin_cobertura",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "duda_sin_cobertura",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Periodo",
                table: "curso",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "curso",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "curso",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "curso",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Modo",
                table: "conversacion",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<Guid>(
                name: "IntentoId",
                table: "conversacion",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreadaEn",
                table: "conversacion",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClaseId",
                table: "conversacion",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "AlumnoId",
                table: "conversacion",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "AgenteId",
                table: "conversacion",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "conversacion",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Titulo",
                table: "clase",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<int>(
                name: "Orden",
                table: "clase",
                type: "integer",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "Inicio",
                table: "clase",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<Guid>(
                name: "CursoId",
                table: "clase",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<bool>(
                name: "AmpliacionPermitida",
                table: "clase",
                type: "boolean",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "INTEGER",
                oldDefaultValue: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "clase",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Tipo",
                table: "archivo_contenido",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "Nombre",
                table: "archivo_contenido",
                type: "character varying(400)",
                maxLength: 400,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 400);

            migrationBuilder.AlterColumn<string>(
                name: "HashSha256",
                table: "archivo_contenido",
                type: "character varying(64)",
                maxLength: 64,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 64);

            migrationBuilder.AlterColumn<string>(
                name: "Estado",
                table: "archivo_contenido",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Error",
                table: "archivo_contenido",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "CreadoEn",
                table: "archivo_contenido",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<Guid>(
                name: "ClaseId",
                table: "archivo_contenido",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "BlobUrl",
                table: "archivo_contenido",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "archivo_contenido",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Texto",
                table: "alternativa",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<Guid>(
                name: "PreguntaId",
                table: "alternativa",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Letra",
                table: "alternativa",
                type: "character varying(1)",
                maxLength: 1,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 1);

            migrationBuilder.AlterColumn<bool>(
                name: "EsCorrecta",
                table: "alternativa",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "alternativa",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "TEXT");

            migrationBuilder.AlterColumn<string>(
                name: "Proveedor",
                table: "agente_ia",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "NombreVisible",
                table: "agente_ia",
                type: "character varying(60)",
                maxLength: 60,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 60);

            migrationBuilder.AlterColumn<string>(
                name: "Modelo",
                table: "agente_ia",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<bool>(
                name: "Habilitado",
                table: "agente_ia",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "INTEGER");

            migrationBuilder.AlterColumn<string>(
                name: "Descripcion",
                table: "agente_ia",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 300,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BaseUrl",
                table: "agente_ia",
                type: "character varying(300)",
                maxLength: 300,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 300);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "agente_ia",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 20);
        }
    }
}
