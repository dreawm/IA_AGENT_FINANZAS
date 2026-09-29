using TutorPreClase.Api.Seguridad;
using TutorPreClase.Application.Llm;

namespace TutorPreClase.Api.Endpoints;

public sealed record CanjeOAuthPeticion(string Code);

/// <summary>
/// Credenciales BYOK del alumno (SDD §7). La unica forma de conectar una es iniciar
/// sesion en el proveedor (RF-24, RF-29): la plataforma no acepta claves pegadas. Todo va
/// contra el usuario del token, y la clave nunca se devuelve.
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
                origen = c.Origen.ToString(),
                estado = c.Estado.ToString(),
                creadaEn = c.CreadaEn,
                ultimoUsoEn = c.UltimoUsoEn
            }));
        });

        // Conexion por inicio de sesion en el proveedor, sin pegar clave (RF-29, SDD §5.1).
        grupo.MapPost("/{agenteId}/oauth/inicio", async (
            string agenteId, HttpContext http, IConexionOAuth oauth, CancellationToken ct) =>
        {
            var url = await oauth.IniciarAsync(http.User.Id(), agenteId, ct);
            return Results.Ok(new { url });
        });

        grupo.MapPost("/{agenteId}/oauth/canje", async (
            string agenteId,
            CanjeOAuthPeticion peticion,
            HttpContext http,
            IConexionOAuth oauth,
            CancellationToken ct) =>
        {
            var credencial = await oauth.CanjearAsync(http.User.Id(), agenteId, peticion.Code, ct);
            return Results.Ok(Resumen(credencial));
        });

        grupo.MapDelete("/{agenteId}", async (
            string agenteId, HttpContext http, IBovedaCredenciales boveda, CancellationToken ct) =>
        {
            await boveda.DesconectarAsync(http.User.Id(), agenteId, ct);
            return Results.NoContent();
        });
    }

    private static object Resumen(CredencialResumen c) => new
    {
        agenteId = c.AgenteId,
        ultimos4 = c.Ultimos4,
        origen = c.Origen.ToString(),
        estado = c.Estado.ToString(),
        creadaEn = c.CreadaEn
    };
}
