using TutorPreClase.Api.Seguridad;
using TutorPreClase.Application.Llm;

namespace TutorPreClase.Api.Endpoints;

public sealed record ConectarCredencialPeticion(string Clave);

/// <summary>
/// Credenciales BYOK del alumno (SDD §7). Todo va contra el usuario del token: nadie
/// puede leer ni tocar la credencial de otro, y la clave nunca se devuelve.
/// </summary>
public static class EndpointsCredenciales
{
    public static void MapearCredenciales(this IEndpointRouteBuilder app)
    {
        var grupo = app.MapGroup("/api/v1/alumno/credenciales").RequireAuthorization();

        grupo.MapGet("/", async (HttpContext http, IBovedaCredenciales boveda, CancellationToken ct) =>
        {
            var credenciales = await boveda.ListarAsync(http.User.Id(), ct);

            return Results.Ok(credenciales.Select(c => new
            {
                agenteId = c.AgenteId,
                ultimos4 = c.Ultimos4,
                estado = c.Estado.ToString(),
                creadaEn = c.CreadaEn,
                ultimoUsoEn = c.UltimoUsoEn
            }));
        });

        grupo.MapPut("/{agenteId}", async (
            string agenteId,
            ConectarCredencialPeticion peticion,
            HttpContext http,
            IBovedaCredenciales boveda,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(peticion.Clave))
                return Results.BadRequest(new { error = "clave_requerida", mensaje = "Pega tu credencial." });

            var credencial = await boveda.ConectarAsync(http.User.Id(), agenteId, peticion.Clave, ct);

            return Results.Ok(new
            {
                agenteId = credencial.AgenteId,
                ultimos4 = credencial.Ultimos4,
                estado = credencial.Estado.ToString(),
                creadaEn = credencial.CreadaEn
            });
        });

        grupo.MapDelete("/{agenteId}", async (
            string agenteId, HttpContext http, IBovedaCredenciales boveda, CancellationToken ct) =>
        {
            await boveda.DesconectarAsync(http.User.Id(), agenteId, ct);
            return Results.NoContent();
        });
    }
}
