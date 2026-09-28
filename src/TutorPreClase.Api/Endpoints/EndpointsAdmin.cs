using Microsoft.EntityFrameworkCore;
using TutorPreClase.Api.Seguridad;
using TutorPreClase.Application.Abstracciones;

namespace TutorPreClase.Api.Endpoints;

public sealed record ConfigurarAgentePeticion(string? Modelo, bool? Habilitado, string? Descripcion);

public static class EndpointsAdmin
{
    public static void MapearAdmin(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/admin")
            .RequireAuthorization(p => p.RequireRole(Roles.Admin));

        grupo.MapGet("/agentes", async (IAppDbContext db, CancellationToken ct) =>
            Results.Ok(await db.Agentes.AsNoTracking().ToListAsync(ct)));

        grupo.MapPut("/agentes/{agenteId}", async (
            string agenteId, ConfigurarAgentePeticion peticion, IAppDbContext db, CancellationToken ct) =>
        {
            var agente = await db.Agentes.FirstOrDefaultAsync(a => a.Id == agenteId, ct);
            if (agente is null) return Results.NotFound();

            if (peticion.Modelo is not null) agente.Modelo = peticion.Modelo;
            if (peticion.Habilitado is not null) agente.Habilitado = peticion.Habilitado.Value;
            if (peticion.Descripcion is not null) agente.Descripcion = peticion.Descripcion;

            await db.SaveChangesAsync(ct);

            return Results.Ok(new
            {
                id = agente.Id,
                modelo = agente.Modelo,
                habilitado = agente.Habilitado
            });
        });
    }
}
