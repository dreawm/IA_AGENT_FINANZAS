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
    DateTimeOffset AbreEn, DateTimeOffset CierraEn, int MaxIntentos, int? MinutosLimite, string ModoFeedback,
    int? PreguntasPorIntento = null);
public sealed record AmpliacionPeticion(bool Permitida);
public sealed record CorregirNivelPeticion(string Nivel);

public static class EndpointsDocente
{
    public static void MapearDocente(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1")
            .RequireAuthorization(p => p.RequireRole(Roles.Docente, Roles.Admin))
            .AddEndpointFilter(SoloSusCursos);

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
            var ampliacion = await db.Clases.Where(c => c.Id == claseId).Select(c => c.AmpliacionPermitida).FirstOrDefaultAsync(ct);

            return Results.Ok(new
            {
                archivos,
                contextoClase = new { tokens = ctx.Tokens, truncado = ctx.Truncado },
                ampliacionPermitida = ampliacion
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
            if (peticion.PreguntasPorIntento is int cantidad) examen.PreguntasPorIntento = Math.Clamp(cantidad, 3, 20);

            await db.SaveChangesAsync(ct);
            return Results.Ok(new { examenId = examen.Id });
        });

        // Las preguntas no se crean ni se aprueban a mano: la IA genera las de cada
        // alumno en cada intento, a partir del material de la clase (RF-04).

        grupo.MapPost("/examenes/{examenId:guid}/publicar", async (
            Guid examenId, IAppDbContext db, CancellationToken ct) =>
        {
            var examen = await db.Examenes.FirstOrDefaultAsync(e => e.Id == examenId, ct);
            if (examen is null) return Results.NotFound();

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

    /// <summary>
    /// Cada profesor administra solo el contenido de sus cursos (RF-34): el curso se deduce
    /// del recurso de la ruta. El administrador ve todos.
    /// </summary>
    private static async ValueTask<object?> SoloSusCursos(EndpointFilterInvocationContext contexto, EndpointFilterDelegate siguiente)
    {
        var http = contexto.HttpContext;
        if (http.User.IsInRole(Roles.Admin)) return await siguiente(contexto);

        var db = http.RequestServices.GetRequiredService<IAppDbContext>();
        var ruta = http.Request.RouteValues;
        var ct = http.RequestAborted;

        Guid? Id(string nombre) => Guid.TryParse(ruta[nombre]?.ToString(), out var id) ? id : null;

        Guid? cursoId = Id("cursoId");
        if (Id("claseId") is Guid claseId)
            cursoId = await db.Clases.Where(c => c.Id == claseId).Select(c => (Guid?)c.CursoId).FirstOrDefaultAsync(ct);
        else if (Id("examenId") is Guid examenId)
            cursoId = await db.Examenes.Where(e => e.Id == examenId).Select(e => (Guid?)e.Clase!.CursoId).FirstOrDefaultAsync(ct);
        else if (Id("archivoId") is Guid archivoId)
            cursoId = await db.Archivos.Where(a => a.Id == archivoId).Select(a => (Guid?)a.Clase!.CursoId).FirstOrDefaultAsync(ct);

        if (cursoId is null) return Results.NotFound();

        var propio = await db.Cursos.AnyAsync(c => c.Id == cursoId && c.DocenteId == http.User.Id(), ct);
        return propio ? await siguiente(contexto) : Results.Forbid();
    }
}
