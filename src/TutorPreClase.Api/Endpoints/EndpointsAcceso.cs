using Microsoft.EntityFrameworkCore;
using TutorPreClase.Api.Seguridad;
using TutorPreClase.Application.Abstracciones;

namespace TutorPreClase.Api.Endpoints;

public sealed record CanjeAccesoPeticion(string Code, string State);
public sealed record AccesoDesarrolloPeticion(Guid UsuarioId);

/// <summary>
/// Pagina de entrada de todos los roles (RF-31): la web pide la URL del proveedor, el
/// usuario inicia sesion alli y la web entrega el codigo de vuelta para recibir su sesion.
/// </summary>
public static class EndpointsAcceso
{
    public static void MapearAcceso(this IEndpointRouteBuilder app, IHostEnvironment entorno)
    {
        var grupo = app.MapGroup("/api/v1/acceso");

        grupo.MapGet("/proveedores", (ConexionIdentidad identidad) => Results.Ok(new
        {
            proveedores = identidad.Disponibles().Select(p => new { id = p.Id, nombre = p.Nombre }),
            desarrollo = !entorno.IsProduction()
        })).AllowAnonymous();

        grupo.MapPost("/{proveedor}/inicio", (string proveedor, ConexionIdentidad identidad) =>
            Results.Ok(new { url = identidad.Iniciar(proveedor) })).AllowAnonymous();

        grupo.MapPost("/{proveedor}/canje", async (
            string proveedor, CanjeAccesoPeticion peticion, ConexionIdentidad identidad, CancellationToken ct) =>
            Results.Ok(await identidad.CanjearAsync(proveedor, peticion.Code, peticion.State, ct))).AllowAnonymous();

        // La web lo usa al recargar la pagina para saber si la sesion sigue viva.
        grupo.MapGet("/yo", async (HttpContext http, IAppDbContext db, CancellationToken ct) =>
        {
            var usuario = await db.Usuarios.AsNoTracking().FirstOrDefaultAsync(u => u.Id == http.User.Id(), ct);
            return usuario is null
                ? Results.Unauthorized()
                : Results.Ok(new UsuarioSesion(usuario.Id, usuario.Nombre, usuario.Email, usuario.Rol.ToString()));
        }).RequireAuthorization();

        if (entorno.IsProduction()) return;

        // Solo fuera de produccion: entrar como un usuario ya registrado, sin proveedor.
        grupo.MapGet("/desarrollo/usuarios", async (IAppDbContext db, CancellationToken ct) =>
            Results.Ok(await db.Usuarios.AsNoTracking()
                .OrderBy(u => u.Rol).ThenBy(u => u.Nombre)
                .Select(u => new { id = u.Id, nombre = u.Nombre, email = u.Email, rol = u.Rol.ToString() })
                .ToListAsync(ct))).AllowAnonymous();

        grupo.MapPost("/desarrollo", async (
            AccesoDesarrolloPeticion peticion, IAppDbContext db, ServicioSesion sesiones, CancellationToken ct) =>
        {
            var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Id == peticion.UsuarioId, ct);
            return usuario is null ? Results.NotFound() : Results.Ok(sesiones.Emitir(usuario));
        }).AllowAnonymous();
    }
}
