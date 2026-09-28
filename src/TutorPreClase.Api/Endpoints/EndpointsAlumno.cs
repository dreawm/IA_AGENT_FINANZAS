using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TutorPreClase.Api.Seguridad;
using TutorPreClase.Api.Sse;
using TutorPreClase.Application.Abstracciones;
using TutorPreClase.Application.Evaluacion;
using TutorPreClase.Application.Llm;
using TutorPreClase.Application.Tutor;
using TutorPreClase.Domain.Entidades;

namespace TutorPreClase.Api.Endpoints;

public sealed record AbrirConversacionPeticion(string AgenteId);
public sealed record MensajePeticion(string Texto);
public sealed record CambiarAgentePeticion(string AgenteId);

public static class EndpointsAlumno
{
    public static void MapearAlumno(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1").RequireAuthorization();

        grupo.MapGet("/agentes", async (
            HttpContext http, IAppDbContext db, IBovedaCredenciales boveda, CancellationToken ct) =>
        {
            var conectados = await boveda.AgentesConectadosAsync(http.User.Id(), ct);

            var agentes = await db.Agentes
                .AsNoTracking()
                .Where(a => a.Habilitado)
                .Select(a => new { id = a.Id, nombre = a.NombreVisible, descripcion = a.Descripcion, consola = a.UrlConsola })
                .ToListAsync(ct);

            // Se listan todos los habilitados: el alumno necesita ver cual puede conectar.
            return Results.Ok(agentes.Select(a => new
            {
                a.id,
                a.nombre,
                a.descripcion,
                a.consola,
                conectado = conectados.Contains(a.id)
            }));
        });

        grupo.MapGet("/alumno/clases", async (HttpContext http, IAppDbContext db, IRelojSistema reloj, CancellationToken ct) =>
        {
            var alumnoId = http.User.Id();

            var cursos = await db.Matriculas
                .AsNoTracking()
                .Where(m => m.UsuarioId == alumnoId)
                .Select(m => m.CursoId)
                .ToListAsync(ct);

            var clases = await db.Clases
                .AsNoTracking()
                .Where(c => cursos.Contains(c.CursoId))
                .OrderBy(c => c.Inicio)
                .Select(c => new
                {
                    claseId = c.Id,
                    titulo = c.Titulo,
                    inicio = c.Inicio,
                    examen = c.Examen == null ? null : new
                    {
                        publicado = c.Examen.Publicado,
                        abreEn = c.Examen.AbreEn,
                        cierraEn = c.Examen.CierraEn
                    }
                })
                .ToListAsync(ct);

            return Results.Ok(clases);
        });

        grupo.MapPost("/clases/{claseId:guid}/conversacion", async (
            Guid claseId,
            AbrirConversacionPeticion peticion,
            HttpContext http,
            IAgenteTutorService tutor,
            CancellationToken ct) =>
        {
            var conversacion = await tutor.AbrirConversacionAsync(claseId, http.User.Id(), peticion.AgenteId, ct);

            return Results.Ok(new
            {
                conversacionId = conversacion.Id,
                modo = conversacion.Modo.ToString(),
                agenteId = conversacion.AgenteId
            });
        });

        grupo.MapPost("/conversaciones/{conversacionId:guid}/mensajes", async (
            Guid conversacionId,
            [FromBody] MensajePeticion peticion,
            HttpContext http,
            IAgenteTutorService tutor,
            IAppDbContext db,
            CancellationToken ct) =>
        {
            if (!await EsDueñoAsync(db, conversacionId, http.User.Id(), ct))
                return Results.Forbid();

            EscritorSse.PrepararCabeceras(http.Response);

            await foreach (var evento in tutor.ProcesarMensajeAsync(conversacionId, peticion.Texto, ct))
                await EscritorSse.EscribirAsync(http.Response, evento, ct);

            return Results.Empty;
        });

        grupo.MapGet("/conversaciones/{conversacionId:guid}/mensajes", async (
            Guid conversacionId, HttpContext http, IAppDbContext db, CancellationToken ct) =>
        {
            if (!await EsDueñoAsync(db, conversacionId, http.User.Id(), ct))
                return Results.Forbid();

            var mensajes = await db.Mensajes
                .AsNoTracking()
                .Where(m => m.ConversacionId == conversacionId && m.Rol != RolMensaje.Herramienta)
                .OrderBy(m => m.CreadoEn)
                .Select(m => new
                {
                    rol = m.Rol.ToString(),
                    texto = m.Texto,
                    agenteId = m.AgenteId,
                    fuentes = m.Fuentes,
                    usaAmpliacion = m.UsaAmpliacion,
                    creadoEn = m.CreadoEn
                })
                .ToListAsync(ct);

            return Results.Ok(mensajes);
        });

        grupo.MapPut("/conversaciones/{conversacionId:guid}/agente", async (
            Guid conversacionId,
            CambiarAgentePeticion peticion,
            HttpContext http,
            IAgenteTutorService tutor,
            IAppDbContext db,
            CancellationToken ct) =>
        {
            if (!await EsDueñoAsync(db, conversacionId, http.User.Id(), ct))
                return Results.Forbid();

            var conversacion = await tutor.CambiarAgenteAsync(conversacionId, peticion.AgenteId, ct);
            return Results.Ok(new { agenteId = conversacion.AgenteId });
        });

        grupo.MapPost("/conversaciones/{conversacionId:guid}/examen", async (
            Guid conversacionId, HttpContext http, IAgenteTutorService tutor, IAppDbContext db, CancellationToken ct) =>
        {
            if (!await EsDueñoAsync(db, conversacionId, http.User.Id(), ct))
                return Results.Forbid();

            var conversacion = await tutor.IniciarExamenAsync(conversacionId, ct);

            return Results.Ok(new
            {
                conversacionId = conversacion.Id,
                intentoId = conversacion.IntentoId,
                modo = conversacion.Modo.ToString()
            });
        });

        grupo.MapGet("/intentos/{intentoId:guid}/resultado", async (
            Guid intentoId, HttpContext http, IServicioExamen examen, IAppDbContext db, CancellationToken ct) =>
        {
            var intento = await db.Intentos.AsNoTracking().FirstOrDefaultAsync(i => i.Id == intentoId, ct);
            if (intento is null) return Results.NotFound();
            if (intento.AlumnoId != http.User.Id() && !http.User.EsDocenteOAdmin()) return Results.Forbid();

            return Results.Ok(await examen.ResultadoAsync(intentoId, ct));
        });
    }

    private static Task<bool> EsDueñoAsync(IAppDbContext db, Guid conversacionId, Guid alumnoId, CancellationToken ct) =>
        db.Conversaciones.AnyAsync(c => c.Id == conversacionId && c.AlumnoId == alumnoId, ct);
}
