using System.Buffers.Text;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using TutorPreClase.Application.Abstracciones;
using TutorPreClase.Domain.Entidades;

namespace TutorPreClase.Api.Seguridad;

/// <summary>Seccion "Acceso": inicio de sesion con la cuenta de la universidad (RF-31).</summary>
public sealed class OpcionesAcceso
{
    public const string Seccion = "Acceso";

    /// <summary>"microsoft" y "google"; solo se ofrecen los que tienen ClientId.</summary>
    public Dictionary<string, OpcionesProveedorIdentidad> Proveedores { get; set; } = [];

    /// <summary>Ruta de la web a la que vuelve cada proveedor: {UrlRetorno}/{proveedor}.</summary>
    public string UrlRetorno { get; set; } = "http://localhost:4200/entrar";

    /// <summary>Correos que entran como administrador aunque nadie los haya dado de alta.</summary>
    public string[] Administradores { get; set; } = [];

    /// <summary>
    /// Correos del profesor: entran como docente, ven todos los cursos y gestionan los
    /// usuarios (en esta etapa el profesor hace de administrador).
    /// </summary>
    public string[] Docentes { get; set; } = [];

    /// <summary>
    /// Con registro abierto, cualquiera que inicie sesión entra como alumno de todos los
    /// cursos, sin que nadie lo dé de alta. Cerrado, solo entran los registrados (RF-33).
    /// </summary>
    public bool RegistroAbierto { get; set; } = true;

    /// <summary>Clave (base64, 32+ bytes) con la que la API firma sus sesiones.</summary>
    public string ClaveSesion { get; set; } = "";

    public int HorasSesion { get; set; } = 12;
}

public sealed class OpcionesProveedorIdentidad
{
    public string Nombre { get; set; } = "";
    public string ClientId { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string UrlAutorizacion { get; set; } = "";
    public string UrlToken { get; set; } = "";

    /// <summary>Emisores validos del id_token (prefijo, para los multi-inquilino).</summary>
    public string[] Emisores { get; set; } = [];
}

public sealed class AccesoException(string codigo, string mensaje, int estado = StatusCodes.Status400BadRequest)
    : Exception(mensaje)
{
    public string Codigo { get; } = codigo;
    public int Estado { get; } = estado;
}

public sealed record UsuarioSesion(Guid Id, string Nombre, string Email, string Rol);

public sealed record Sesion(string Token, UsuarioSesion Usuario);

/// <summary>
/// Firma y valida las sesiones propias de la API. El proveedor (Microsoft o Google) solo
/// dice quien es la persona; el rol lo decide la plataforma (RF-31, RF-33).
/// </summary>
public sealed class ServicioSesion(IOptions<OpcionesAcceso> opciones, IHostEnvironment entorno)
{
    public const string Emisor = "tutorpreclase";
    public const string Esquema = "Sesion";

    private readonly SymmetricSecurityKey _clave = Clave(opciones.Value.ClaveSesion, entorno);

    public SymmetricSecurityKey ClaveFirma => _clave;

    public Sesion Emitir(Usuario usuario)
    {
        var token = new JsonWebTokenHandler().CreateToken(new SecurityTokenDescriptor
        {
            Issuer = Emisor,
            Audience = Emisor,
            Expires = DateTime.UtcNow.AddHours(Math.Max(1, opciones.Value.HorasSesion)),
            SigningCredentials = new SigningCredentials(_clave, SecurityAlgorithms.HmacSha256),
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
                new Claim("role", usuario.Rol.ToString()),
                new Claim(JwtRegisteredClaimNames.Name, usuario.Nombre),
                new Claim(JwtRegisteredClaimNames.Email, usuario.Email)
            ])
        });

        return new Sesion(token, new UsuarioSesion(usuario.Id, usuario.Nombre, usuario.Email, usuario.Rol.ToString()));
    }

    private static SymmetricSecurityKey Clave(string configurada, IHostEnvironment entorno)
    {
        if (!string.IsNullOrWhiteSpace(configurada))
            return new SymmetricSecurityKey(Convert.FromBase64String(configurada));

        if (entorno.IsProduction())
            throw new InvalidOperationException("Falta Acceso:ClaveSesion: la API no puede firmar sesiones.");

        // En desarrollo, una clave por proceso: al reiniciar hay que volver a entrar.
        return new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32));
    }
}

/// <summary>
/// OpenID Connect con codigo de autorizacion + PKCE contra Microsoft o Google (RF-31). El
/// canje lo hace la API directamente con el proveedor por TLS, asi que el id_token que
/// recibe se valida por sus afirmaciones (emisor, audiencia, vigencia, nonce) sin viajar
/// por el navegador.
/// </summary>
public sealed class ConexionIdentidad(
    IMemoryCache cache,
    IHttpClientFactory http,
    IAppDbContext db,
    ServicioSesion sesiones,
    IOptions<OpcionesAcceso> opciones,
    ILogger<ConexionIdentidad> log)
{
    private static readonly TimeSpan Vigencia = TimeSpan.FromMinutes(10);

    public IEnumerable<(string Id, string Nombre)> Disponibles() =>
        opciones.Value.Proveedores
            .Where(p => !string.IsNullOrWhiteSpace(p.Value.ClientId))
            .Select(p => (p.Key, p.Value.Nombre));

    public string Iniciar(string proveedor)
    {
        var config = Proveedor(proveedor);

        var estado = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(24));
        var verificador = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));
        var nonce = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(16));
        var desafio = Base64Url.EncodeToString(SHA256.HashData(Encoding.ASCII.GetBytes(verificador)));

        cache.Set(ClaveCache(estado), new Pendiente(proveedor, verificador, nonce), Vigencia);

        return config.UrlAutorizacion +
               $"?client_id={Uri.EscapeDataString(config.ClientId)}" +
               "&response_type=code" +
               $"&redirect_uri={Uri.EscapeDataString(Retorno(proveedor))}" +
               $"&scope={Uri.EscapeDataString("openid email profile")}" +
               $"&state={estado}&nonce={nonce}" +
               $"&code_challenge={desafio}&code_challenge_method=S256" +
               "&prompt=select_account";
    }

    public async Task<Sesion> CanjearAsync(string proveedor, string codigo, string estado, CancellationToken ct)
    {
        var config = Proveedor(proveedor);

        // El estado es de un solo uso y liga el retorno con quien inicio el acceso.
        if (!cache.TryGetValue(ClaveCache(estado), out Pendiente? pendiente) || pendiente is null ||
            pendiente.Proveedor != proveedor)
            throw new AccesoException("acceso_vencido", "El inicio de sesión venció. Vuelve a intentarlo.");

        cache.Remove(ClaveCache(estado));

        var idToken = await PedirTokenAsync(proveedor, config, codigo, pendiente.Verificador, ct);
        var (email, nombre) = Validar(config, idToken, pendiente.Nonce);

        return sesiones.Emitir(await UsuarioAsync(email, nombre, ct));
    }

    /// <summary>
    /// Los correos de <c>Acceso:Docentes</c> entran como docente y los de
    /// <c>Acceso:Administradores</c> como administrador. Con registro abierto, el resto entra
    /// como alumno; cerrado, solo quien esta registrado (RF-33).
    /// </summary>
    private async Task<Usuario> UsuarioAsync(string email, string nombre, CancellationToken ct)
    {
        var o = opciones.Value;
        var esDocente = Figura(o.Docentes, email);
        var esAdmin = Figura(o.Administradores, email);

        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == email, ct);

        if (usuario is null)
        {
            if (!esDocente && !esAdmin && !o.RegistroAbierto)
                throw new AccesoException("no_registrado",
                    $"La cuenta {email} no está registrada en la plataforma. Pide al administrador que te dé de alta.",
                    StatusCodes.Status403Forbidden);

            usuario = new Usuario
            {
                Email = email,
                Nombre = nombre.Length > 0 ? nombre : email,
                Rol = esAdmin ? RolUsuario.Admin : esDocente ? RolUsuario.Docente : RolUsuario.Alumno
            };
            db.Usuarios.Add(usuario);
            log.LogInformation("{Email} registrado en su primer acceso como {Rol}", email, usuario.Rol);
        }
        else if (esDocente && usuario.Rol == RolUsuario.Alumno)
        {
            // Si el profesor entró antes de figurar en la configuración, quedó como alumno.
            usuario.Rol = RolUsuario.Docente;
        }

        // El profesor y, con registro abierto, los alumnos ven todos los cursos, incluidos
        // los que aparecieron en la carpeta después de su último acceso.
        if (esDocente || (o.RegistroAbierto && usuario.Rol == RolUsuario.Alumno))
            await MatricularEnTodosAsync(usuario, ct);

        await db.SaveChangesAsync(ct);
        return usuario;
    }

    private async Task MatricularEnTodosAsync(Usuario usuario, CancellationToken ct)
    {
        var tiene = await db.Matriculas.Where(m => m.UsuarioId == usuario.Id).Select(m => m.CursoId).ToListAsync(ct);
        var faltan = await db.Cursos.Where(c => !tiene.Contains(c.Id)).Select(c => c.Id).ToListAsync(ct);

        foreach (var cursoId in faltan)
            db.Matriculas.Add(new Matricula { Usuario = usuario, CursoId = cursoId });
    }

    private static bool Figura(IEnumerable<string> correos, string email) =>
        correos.Any(c => string.Equals(c.Trim(), email, StringComparison.OrdinalIgnoreCase));

    private async Task<string> PedirTokenAsync(
        string proveedor, OpcionesProveedorIdentidad config, string codigo, string verificador, CancellationToken ct)
    {
        using var respuesta = await http.CreateClient("identidad").PostAsync(config.UrlToken, new FormUrlEncodedContent(
            new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = codigo,
                ["redirect_uri"] = Retorno(proveedor),
                ["client_id"] = config.ClientId,
                ["client_secret"] = config.ClientSecret,
                ["code_verifier"] = verificador
            }), ct);

        // El cuerpo no se registra: si el canje sale bien, trae tokens.
        if (!respuesta.IsSuccessStatusCode)
        {
            log.LogWarning("{Proveedor} rechazo el canje del codigo con {Estado}", proveedor, (int)respuesta.StatusCode);
            throw new AccesoException("acceso_rechazado", $"{config.Nombre} no aceptó el inicio de sesión. Vuelve a intentarlo.");
        }

        using var cuerpo = JsonDocument.Parse(await respuesta.Content.ReadAsStringAsync(ct));
        return cuerpo.RootElement.TryGetProperty("id_token", out var token) && token.GetString() is { } valor
            ? valor
            : throw new AccesoException("acceso_rechazado", $"{config.Nombre} no devolvió tu identidad.");
    }

    public static (string Email, string Nombre) Validar(OpcionesProveedorIdentidad config, string idToken, string nonce)
    {
        JsonWebToken token;
        try
        {
            token = new JsonWebToken(idToken);
        }
        catch (Exception ex) when (ex is ArgumentException or SecurityTokenMalformedException)
        {
            throw new AccesoException("acceso_rechazado", "La identidad recibida no es válida.");
        }

        var emisorValido = config.Emisores.Any(e => token.Issuer.StartsWith(e, StringComparison.OrdinalIgnoreCase));
        var audienciaValida = token.Audiences.Contains(config.ClientId);
        var vigente = token.ValidTo > DateTime.UtcNow.AddMinutes(-2);
        var mismoNonce = token.TryGetPayloadValue<string>("nonce", out var n) && n == nonce;

        if (!emisorValido || !audienciaValida || !vigente || !mismoNonce)
            throw new AccesoException("acceso_rechazado", "La identidad recibida no es válida.");

        // Google marca si el correo esta verificado; Microsoft usa el nombre de la cuenta.
        if (token.TryGetPayloadValue<bool>("email_verified", out var verificado) && !verificado)
            throw new AccesoException("acceso_rechazado", "Tu correo no está verificado en tu cuenta.");

        var email = token.TryGetPayloadValue<string>("email", out var e) && !string.IsNullOrWhiteSpace(e) ? e
                  : token.TryGetPayloadValue<string>("preferred_username", out var p) ? p : "";

        if (!email.Contains('@'))
            throw new AccesoException("acceso_rechazado", "Tu cuenta no tiene un correo asociado.");

        var nombre = token.TryGetPayloadValue<string>("name", out var nom) ? nom.Trim() : "";
        return (email.Trim().ToLowerInvariant(), nombre);
    }

    private OpcionesProveedorIdentidad Proveedor(string proveedor) =>
        opciones.Value.Proveedores.TryGetValue(proveedor, out var config) && !string.IsNullOrWhiteSpace(config.ClientId)
            ? config
            : throw new AccesoException("proveedor_no_disponible", "Ese inicio de sesión no está configurado.");

    private string Retorno(string proveedor) => $"{opciones.Value.UrlRetorno.TrimEnd('/')}/{proveedor}";

    private static string ClaveCache(string estado) => $"acceso:{estado}";

    private sealed record Pendiente(string Proveedor, string Verificador, string Nonce);
}
