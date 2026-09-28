using Microsoft.EntityFrameworkCore;
using TutorPreClase.Api.Seguridad;
using TutorPreClase.Application.Abstracciones;
using TutorPreClase.Application.Contenido;
using TutorPreClase.Application.Nivel;
using TutorPreClase.Application.Reportes;
using TutorPreClase.Domain.Entidades;
using TutorPreClase.Infrastructure.Contenido;

namespace TutorPreClase.Api.Endpoints;

public sealed record CrearClasePeticion(string Titulo, DateTimeOffset Inicio, int Orden);
public sealed record ConfigurarExamenPeticion(
    DateTimeOffset AbreEn, DateTimeOffset CierraEn, int MaxIntentos, int? MinutosLimite, string ModoFeedback);
public sealed record AlternativaPeticion(string Letra, string Texto, bool EsCorrecta);
public sealed record CrearPreguntaPeticion(
    string Enunciado, string Justificacion, string Tema, string? Nivel, int Orden,
    bool Aprobada, List<AlternativaPeticion> Alternativas);
public sealed record AmpliacionPeticion(bool Permitida);
public sealed record CorregirNivelPeticion(string Nivel);

public static class EndpointsDocente
{
    public static void MapearDocente(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1")
            .RequireAuthorization(p => p.RequireRole(Roles.Docente, Roles.Admin));

        grupo.MapPost("/cursos/{cursoId:guid}/clases", async (
            Guid cursoId, CrearClasePeticion peticion, IAppDbContext db, CancellationToken ct) =>
        {
            var clase = new Clase
            {
                CursoId = cursoId,
                Titulo = peticion.Titulo,
                Inicio = peticion.Inicio,
                Orden = peticion.Orden
            };

            db.Clases.Add(clase);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/clases/{clase.Id}", new { claseId = clase.Id });
        });

        grupo.MapPost("/clases/{claseId:guid}/contenido", async (
            Guid claseId,
            HttpRequest peticion,
            IAppDbContext db,
            IServicioContenido contenido,
            IColaExtraccion cola,
            CancellationToken ct) =>
        {
            if (!peticion.HasFormContentType) return Results.BadRequest(new { error = "Se espera multipart/form-data." });

            var clase = await db.Clases.AsNoTracking().FirstOrDefaultAsync(c => c.Id == claseId, ct);
            if (clase is null) return Results.NotFound();

            var formulario = await peticion.ReadFormAsync(ct);
            var subidos = new List<object>();

            foreach (var archivo in formulario.Files)
            {
                await using var flujo = archivo.OpenReadStream();
                var resultado = await contenido.SubirAsync(clase.CursoId, claseId, archivo.FileName, flujo, ct);

                // La API nunca espera a la extraccion: encola y responde (SDD §3).
                if (!resultado.Duplicado) await cola.EncolarAsync(resultado.ArchivoId, ct);

                subidos.Add(new { archivoId = resultado.ArchivoId, nombre = resultado.Nombre, duplicado = resultado.Duplicado });
            }

            return Results.Accepted(value: subidos);
        }).DisableAntiforgery();

        grupo.MapGet("/clases/{claseId:guid}/contenido", async (
            Guid claseId, IAppDbContext db, IContextoClaseService contexto, CancellationToken ct) =>
        {
            var archivos = await db.Archivos
                .AsNoTracking()
                .Where(a => a.ClaseId == claseId)
                .OrderBy(a => a.Nombre)
                .Select(a => new
                {
                    archivoId = a.Id,
                    nombre = a.Nombre,
                    estado = a.Estado.ToString(),
                    error = a.Error,
                    paginas = a.Paginas.Count
                })
                .ToListAsync(ct);

            var ctx = await contexto.ObtenerAsync(claseId, ct);

            return Results.Ok(new
            {
                archivos,
                contextoClase = new { tokens = ctx.Tokens, truncado = ctx.Truncado }
            });
        });

        grupo.MapDelete("/contenido/{archivoId:guid}", async (
            Guid archivoId, IServicioContenido contenido, CancellationToken ct) =>
        {
            await contenido.EliminarAsync(archivoId, ct);
            return Results.NoContent();
        });

        grupo.MapPut("/clases/{claseId:guid}/ampliacion", async (
            Guid claseId, AmpliacionPeticion peticion, IAppDbContext db, CancellationToken ct) =>
        {
            var clase = await db.Clases.FirstOrDefaultAsync(c => c.Id == claseId, ct);
            if (clase is null) return Results.NotFound();

            clase.AmpliacionPermitida = peticion.Permitida;
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { ampliacionPermitida = clase.AmpliacionPermitida });
        });

        grupo.MapPut("/clases/{claseId:guid}/examen", async (
            Guid claseId, ConfigurarExamenPeticion peticion, IAppDbContext db, CancellationToken ct) =>
        {
            if (!Enum.TryParse<ModoFeedback>(peticion.ModoFeedback, out var modo))
                return Results.BadRequest(new { error = "modo_feedback_invalido" });

            var examen = await db.Examenes.FirstOrDefaultAsync(e => e.ClaseId == claseId, ct);

            if (examen is null)
            {
                examen = new Examen { ClaseId = claseId };
                db.Examenes.Add(examen);
            }

            examen.AbreEn = peticion.AbreEn;
            examen.CierraEn = peticion.CierraEn;
            examen.MaxIntentos = peticion.MaxIntentos;
            examen.MinutosLimite = peticion.MinutosLimite;
            examen.ModoFeedback = modo;

            await db.SaveChangesAsync(ct);
            return Results.Ok(new { examenId = examen.Id });
        });

        grupo.MapPost("/examenes/{examenId:guid}/preguntas", async (
            Guid examenId, CrearPreguntaPeticion peticion, IAppDbContext db, CancellationToken ct) =>
        {
            if (peticion.Alternativas.Count(a => a.EsCorrecta) != 1)
                return Results.BadRequest(new { error = "Debe haber exactamente una alternativa correcta." });

            NivelAlumnoValor? nivel = Enum.TryParse<NivelAlumnoValor>(peticion.Nivel, out var n) ? n : null;

            var pregunta = new Pregunta
            {
                ExamenId = examenId,
                Enunciado = peticion.Enunciado,
                Justificacion = peticion.Justificacion,
                Tema = peticion.Tema,
                Nivel = nivel,
                Orden = peticion.Orden,
                Origen = OrigenPregunta.Docente,
                Aprobada = peticion.Aprobada,
                Alternativas = peticion.Alternativas
                    .Select(a => new Alternativa { Letra = a.Letra, Texto = a.Texto, EsCorrecta = a.EsCorrecta })
                    .ToList()
            };

            db.Preguntas.Add(pregunta);
            await db.SaveChangesAsync(ct);

            return Results.Created($"/api/v1/preguntas/{pregunta.Id}", new { preguntaId = pregunta.Id });
        });

        grupo.MapPost("/preguntas/{preguntaId:guid}/aprobar", async (
            Guid preguntaId, IAppDbContext db, CancellationToken ct) =>
        {
            var pregunta = await db.Preguntas.FirstOrDefaultAsync(p => p.Id == preguntaId, ct);
            if (pregunta is null) return Results.NotFound();

            pregunta.Aprobada = true;
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { preguntaId, aprobada = true });
        });

        grupo.MapPost("/examenes/{examenId:guid}/publicar", async (
            Guid examenId, IAppDbContext db, CancellationToken ct) =>
        {
            var examen = await db.Examenes
                .Include(e => e.Preguntas)
                .FirstOrDefaultAsync(e => e.Id == examenId, ct);

            if (examen is null) return Results.NotFound();

            if (!examen.Preguntas.Any(p => p.Aprobada))
                return Results.BadRequest(new { error = "El examen no tiene preguntas aprobadas." });

            examen.Publicado = true;
            await db.SaveChangesAsync(ct);

            return Results.Ok(new { examenId, publicado = true });
        });

        grupo.MapGet("/clases/{claseId:guid}/reporte", async (
            Guid claseId, IServicioReporte reportes, CancellationToken ct) =>
        {
            var reporte = await reportes.DeClaseAsync(claseId, ct);
            return reporte is null ? Results.NotFound() : Results.Ok(reporte);
        });

        grupo.MapGet("/clases/{claseId:guid}/niveles", async (
            Guid claseId, IServicioReporte reportes, CancellationToken ct) =>
        {
            var reporte = await reportes.DeClaseAsync(claseId, ct);
            return reporte is null ? Results.NotFound() : Results.Ok(reporte.Niveles);
        });

        grupo.MapPut("/clases/{claseId:guid}/niveles/{alumnoId:guid}", async (
            Guid claseId, Guid alumnoId, CorregirNivelPeticion peticion, INivelService nivel, CancellationToken ct) =>
        {
            if (!Enum.TryParse<NivelAlumnoValor>(peticion.Nivel, out var valor))
                return Results.BadRequest(new { error = "nivel_invalido" });

            var registro = await nivel.CorregirAsync(alumnoId, claseId, valor, ct);

            return Results.Ok(new { nivel = registro.Nivel.ToString(), origen = registro.Origen.ToString() });
        });
    }
}
