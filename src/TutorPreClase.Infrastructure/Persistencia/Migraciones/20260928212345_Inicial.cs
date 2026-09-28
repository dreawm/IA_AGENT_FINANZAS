using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TutorPreClase.Infrastructure.Persistencia.Migraciones
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "agente_ia",
                columns: table => new
                {
                    Id = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    NombreVisible = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    Proveedor = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Modelo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    BaseUrl = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true),
                    Habilitado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agente_ia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "curso",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Periodo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_curso", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "duda_sin_cobertura",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversacionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Texto = table.Column<string>(type: "text", nullable: false),
                    Tema = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    RespondidaConAmpliacion = table.Column<bool>(type: "boolean", nullable: false),
                    CreadoEn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_duda_sin_cobertura", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "nivel_alumno",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    AlumnoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nivel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TemasDebiles = table.Column<string>(type: "jsonb", nullable: false),
                    Origen = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ActualizadoEn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_nivel_alumno", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "usuario",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Rol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    AgentePreferido = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "clase",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CursoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Inicio = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false),
                    AmpliacionPermitida = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_clase", x => x.Id);
                    table.ForeignKey(
                        name: "FK_clase_curso_CursoId",
                        column: x => x.CursoId,
                        principalTable: "curso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "matricula",
                columns: table => new
                {
                    UsuarioId = table.Column<Guid>(type: "uuid", nullable: false),
                    CursoId = table.Column<Guid>(type: "uuid", nullable: false),
                    RolEnCurso = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matricula", x => new { x.UsuarioId, x.CursoId });
                    table.ForeignKey(
                        name: "FK_matricula_curso_CursoId",
                        column: x => x.CursoId,
                        principalTable: "curso",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_matricula_usuario_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "archivo_contenido",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(400)", maxLength: 400, nullable: false),
                    Tipo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    BlobUrl = table.Column<string>(type: "text", nullable: false),
                    HashSha256 = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Error = table.Column<string>(type: "text", nullable: true),
                    CreadoEn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_archivo_contenido", x => x.Id);
                    table.ForeignKey(
                        name: "FK_archivo_contenido_clase_ClaseId",
                        column: x => x.ClaseId,
                        principalTable: "clase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "examen",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    AbreEn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CierraEn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    MaxIntentos = table.Column<int>(type: "integer", nullable: false),
                    MinutosLimite = table.Column<int>(type: "integer", nullable: true),
                    ModoFeedback = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Publicado = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_examen", x => x.Id);
                    table.ForeignKey(
                        name: "FK_examen_clase_ClaseId",
                        column: x => x.ClaseId,
                        principalTable: "clase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pagina_contenido",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ArchivoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    Pagina = table.Column<int>(type: "integer", nullable: false),
                    Texto = table.Column<string>(type: "text", nullable: false),
                    Tokens = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pagina_contenido", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pagina_contenido_archivo_contenido_ArchivoId",
                        column: x => x.ArchivoId,
                        principalTable: "archivo_contenido",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "intento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamenId = table.Column<Guid>(type: "uuid", nullable: false),
                    AlumnoId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgenteId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Modelo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Estado = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Inicio = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Envio = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Puntaje = table.Column<decimal>(type: "numeric(4,1)", precision: 4, scale: 1, nullable: true),
                    Porcentaje = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_intento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_intento_examen_ExamenId",
                        column: x => x.ExamenId,
                        principalTable: "examen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pregunta",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExamenId = table.Column<Guid>(type: "uuid", nullable: false),
                    Enunciado = table.Column<string>(type: "text", nullable: false),
                    Justificacion = table.Column<string>(type: "text", nullable: false),
                    Tema = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Nivel = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: true),
                    Orden = table.Column<int>(type: "integer", nullable: false),
                    Origen = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Aprobada = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pregunta", x => x.Id);
                    table.ForeignKey(
                        name: "FK_pregunta_examen_ExamenId",
                        column: x => x.ExamenId,
                        principalTable: "examen",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "conversacion",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ClaseId = table.Column<Guid>(type: "uuid", nullable: false),
                    AlumnoId = table.Column<Guid>(type: "uuid", nullable: false),
                    AgenteId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Modo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IntentoId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreadaEn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_conversacion", x => x.Id);
                    table.ForeignKey(
                        name: "FK_conversacion_clase_ClaseId",
                        column: x => x.ClaseId,
                        principalTable: "clase",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_conversacion_intento_IntentoId",
                        column: x => x.IntentoId,
                        principalTable: "intento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "alternativa",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PreguntaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Letra = table.Column<string>(type: "character varying(1)", maxLength: 1, nullable: false),
                    Texto = table.Column<string>(type: "text", nullable: false),
                    EsCorrecta = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_alternativa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_alternativa_pregunta_PreguntaId",
                        column: x => x.PreguntaId,
                        principalTable: "pregunta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "pregunta_referencia",
                columns: table => new
                {
                    PreguntaId = table.Column<Guid>(type: "uuid", nullable: false),
                    PaginaId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pregunta_referencia", x => new { x.PreguntaId, x.PaginaId });
                    table.ForeignKey(
                        name: "FK_pregunta_referencia_pagina_contenido_PaginaId",
                        column: x => x.PaginaId,
                        principalTable: "pagina_contenido",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_pregunta_referencia_pregunta_PreguntaId",
                        column: x => x.PreguntaId,
                        principalTable: "pregunta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "respuesta_intento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    IntentoId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreguntaId = table.Column<Guid>(type: "uuid", nullable: false),
                    AlternativaId = table.Column<Guid>(type: "uuid", nullable: false),
                    EsCorrecta = table.Column<bool>(type: "boolean", nullable: false),
                    TextoOriginalAlumno = table.Column<string>(type: "text", nullable: false),
                    CreadoEn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_respuesta_intento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_respuesta_intento_intento_IntentoId",
                        column: x => x.IntentoId,
                        principalTable: "intento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_respuesta_intento_pregunta_PreguntaId",
                        column: x => x.PreguntaId,
                        principalTable: "pregunta",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "mensaje_chat",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConversacionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PreguntaId = table.Column<Guid>(type: "uuid", nullable: true),
                    AgenteId = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Rol = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Texto = table.Column<string>(type: "text", nullable: false),
                    Herramienta = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: true),
                    Fuentes = table.Column<string>(type: "jsonb", nullable: true),
                    UsaAmpliacion = table.Column<bool>(type: "boolean", nullable: false),
                    TokensEntrada = table.Column<int>(type: "integer", nullable: false),
                    TokensSalida = table.Column<int>(type: "integer", nullable: false),
                    CreadoEn = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_mensaje_chat", x => x.Id);
                    table.ForeignKey(
                        name: "FK_mensaje_chat_conversacion_ConversacionId",
                        column: x => x.ConversacionId,
                        principalTable: "conversacion",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_alternativa_PreguntaId",
                table: "alternativa",
                column: "PreguntaId");

            migrationBuilder.CreateIndex(
                name: "IX_archivo_contenido_ClaseId_HashSha256",
                table: "archivo_contenido",
                columns: new[] { "ClaseId", "HashSha256" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_clase_CursoId",
                table: "clase",
                column: "CursoId");

            migrationBuilder.CreateIndex(
                name: "IX_conversacion_ClaseId_AlumnoId",
                table: "conversacion",
                columns: new[] { "ClaseId", "AlumnoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_conversacion_IntentoId",
                table: "conversacion",
                column: "IntentoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_duda_sin_cobertura_ClaseId",
                table: "duda_sin_cobertura",
                column: "ClaseId");

            migrationBuilder.CreateIndex(
                name: "IX_examen_ClaseId",
                table: "examen",
                column: "ClaseId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_intento_ExamenId_AlumnoId",
                table: "intento",
                columns: new[] { "ExamenId", "AlumnoId" });

            migrationBuilder.CreateIndex(
                name: "IX_matricula_CursoId",
                table: "matricula",
                column: "CursoId");

            migrationBuilder.CreateIndex(
                name: "IX_mensaje_chat_ConversacionId_CreadoEn",
                table: "mensaje_chat",
                columns: new[] { "ConversacionId", "CreadoEn" });

            migrationBuilder.CreateIndex(
                name: "IX_nivel_alumno_AlumnoId_ClaseId",
                table: "nivel_alumno",
                columns: new[] { "AlumnoId", "ClaseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pagina_contenido_ArchivoId_Pagina",
                table: "pagina_contenido",
                columns: new[] { "ArchivoId", "Pagina" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_pagina_contenido_ClaseId",
                table: "pagina_contenido",
                column: "ClaseId");

            migrationBuilder.CreateIndex(
                name: "IX_pregunta_ExamenId",
                table: "pregunta",
                column: "ExamenId");

            migrationBuilder.CreateIndex(
                name: "IX_pregunta_referencia_PaginaId",
                table: "pregunta_referencia",
                column: "PaginaId");

            migrationBuilder.CreateIndex(
                name: "IX_respuesta_intento_IntentoId_PreguntaId",
                table: "respuesta_intento",
                columns: new[] { "IntentoId", "PreguntaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_respuesta_intento_PreguntaId",
                table: "respuesta_intento",
                column: "PreguntaId");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_Email",
                table: "usuario",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agente_ia");

            migrationBuilder.DropTable(
                name: "alternativa");

            migrationBuilder.DropTable(
                name: "duda_sin_cobertura");

            migrationBuilder.DropTable(
                name: "matricula");

            migrationBuilder.DropTable(
                name: "mensaje_chat");

            migrationBuilder.DropTable(
                name: "nivel_alumno");

            migrationBuilder.DropTable(
                name: "pregunta_referencia");

            migrationBuilder.DropTable(
                name: "respuesta_intento");

            migrationBuilder.DropTable(
                name: "usuario");

            migrationBuilder.DropTable(
                name: "conversacion");

            migrationBuilder.DropTable(
                name: "pagina_contenido");

            migrationBuilder.DropTable(
                name: "pregunta");

            migrationBuilder.DropTable(
                name: "intento");

            migrationBuilder.DropTable(
                name: "archivo_contenido");

            migrationBuilder.DropTable(
                name: "examen");

            migrationBuilder.DropTable(
                name: "clase");

            migrationBuilder.DropTable(
                name: "curso");
        }
    }
}
