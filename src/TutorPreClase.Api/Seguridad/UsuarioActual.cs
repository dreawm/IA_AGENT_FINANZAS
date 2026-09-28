using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using TutorPreClase.Domain.Entidades;

namespace TutorPreClase.Api.Seguridad;

public static class Roles
{
    public const string Alumno = nameof(RolUsuario.Alumno);
    public const string Docente = nameof(RolUsuario.Docente);
    public const string Admin = nameof(RolUsuario.Admin);
}

public static class UsuarioActual
{
    /// <summary>
    /// Id del usuario autenticado. Nunca se toma del cuerpo de la peticion: un alumno
    /// solo accede a lo suyo (SDD §9.1).
    /// </summary>
    public static Guid Id(this ClaimsPrincipal principal)
    {
        var valor = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? principal.FindFirstValue("sub");

        return Guid.TryParse(valor, out var id)
            ? id
            : throw new UnauthorizedAccessException("El token no trae un identificador de usuario valido.");
    }

    public static bool EsDocenteOAdmin(this ClaimsPrincipal principal) =>
        principal.IsInRole(Roles.Docente) || principal.IsInRole(Roles.Admin);
}

public sealed class OpcionesAutenticacionDesarrollo : AuthenticationSchemeOptions;

/// <summary>
/// Autenticacion de desarrollo por cabeceras (X-Usuario-Id / X-Usuario-Rol) para poder
/// levantar la API sin un proveedor OIDC. Solo se registra fuera de Produccion.
/// </summary>
public sealed class ManejadorAutenticacionDesarrollo(
    IOptionsMonitor<OpcionesAutenticacionDesarrollo> opciones,
    ILoggerFactory logs,
    UrlEncoder codificador)
    : AuthenticationHandler<OpcionesAutenticacionDesarrollo>(opciones, logs, codificador)
{
    public const string Esquema = "Desarrollo";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Usuario-Id", out var id) || !Guid.TryParse(id, out var usuarioId))
            return Task.FromResult(AuthenticateResult.NoResult());

        var rol = Request.Headers.TryGetValue("X-Usuario-Rol", out var valor) && valor.Count > 0
            ? valor[0]!
            : Roles.Alumno;

        var identidad = new ClaimsIdentity(
        [
            new Claim(ClaimTypes.NameIdentifier, usuarioId.ToString()),
            new Claim(ClaimTypes.Role, rol)
        ], Esquema);

        var ticket = new AuthenticationTicket(new ClaimsPrincipal(identidad), Esquema);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
