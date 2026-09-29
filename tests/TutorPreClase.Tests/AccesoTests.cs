using System.Buffers.Text;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using TutorPreClase.Api.Seguridad;
using TutorPreClase.Domain.Entidades;
using TutorPreClase.Tests.Infraestructura;

namespace TutorPreClase.Tests;

/// <summary>Proveedor de identidad simulado: devuelve el id_token que el test le fije.</summary>
public sealed class ProveedorIdentidadSimulado : HttpMessageHandler, IHttpClientFactory
{
    public string IdToken { get; set; } = "";
    public HttpStatusCode Estado { get; set; } = HttpStatusCode.OK;
    public string? CuerpoRecibido { get; private set; }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage peticion, CancellationToken ct)
    {
        CuerpoRecibido = peticion.Content is null ? null : await peticion.Content.ReadAsStringAsync(ct);
        return new HttpResponseMessage(Estado) { Content = JsonContent.Create(new { id_token = IdToken }) };
    }

    public HttpClient CreateClient(string nombre) => new(this, disposeHandler: false);
}

public class AccesoTests
{
    private static readonly OpcionesProveedorIdentidad Google = new()
    {
        Nombre = "Google",
        ClientId = "cliente-google",
        ClientSecret = "secreto",
        UrlAutorizacion = "https://accounts.google.com/o/oauth2/v2/auth",
        UrlToken = "https://oauth2.googleapis.com/token",
        Emisores = ["https://accounts.google.com"]
    };

    private static readonly OpcionesProveedorIdentidad Microsoft = new()
    {
        Nombre = "Microsoft",
        ClientId = "cliente-ms",
        ClientSecret = "secreto",
        UrlAutorizacion = "https://login.microsoftonline.com/organizations/oauth2/v2.0/authorize",
        UrlToken = "https://login.microsoftonline.com/organizations/oauth2/v2.0/token",
        Emisores = ["https://login.microsoftonline.com/"]
    };

    /// <summary>Un id_token sin firma: la API lo recibe del proveedor por TLS y valida sus afirmaciones.</summary>
    public static string IdToken(object carga)
    {
        static string B64(object o) => Base64Url.EncodeToString(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(o)));
        return $"{B64(new { alg = "none", typ = "JWT" })}.{B64(carga)}.";
    }

    private static object Carga(string aud = "cliente-google", string iss = "https://accounts.google.com",
        string nonce = "n1", long? exp = null, string? email = "ana@upc.edu.pe", bool verificado = true) => new
    {
        iss, aud, nonce, email, email_verified = verificado, name = "Ana Pérez",
        exp = exp ?? DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds()
    };

    [Fact]
    public void Un_id_token_valido_da_el_correo_en_minusculas_y_el_nombre()
    {
        var (email, nombre) = ConexionIdentidad.Validar(Google, IdToken(Carga(email: "Ana@UPC.edu.pe")), "n1");

        Assert.Equal(("ana@upc.edu.pe", "Ana Pérez"), (email, nombre));
    }

    [Theory]
    [InlineData("otra-app", "https://accounts.google.com", "n1", 5, true)]     // audiencia ajena
    [InlineData("cliente-google", "https://evil.example", "n1", 5, true)]     // emisor ajeno
    [InlineData("cliente-google", "https://accounts.google.com", "x", 5, true)] // nonce repetido de otro inicio
    [InlineData("cliente-google", "https://accounts.google.com", "n1", -10, true)] // vencido
    [InlineData("cliente-google", "https://accounts.google.com", "n1", 5, false)]  // correo sin verificar
    public void Se_rechaza_una_identidad_que_no_cumple(string aud, string iss, string nonce, int minutos, bool verificado)
    {
        var token = IdToken(Carga(aud, iss, nonce, DateTimeOffset.UtcNow.AddMinutes(minutos).ToUnixTimeSeconds(), verificado: verificado));

        var error = Assert.Throws<AccesoException>(() => ConexionIdentidad.Validar(Google, token, "n1"));
        Assert.Equal("acceso_rechazado", error.Codigo);
    }

    [Fact]
    public void Microsoft_sin_email_usa_el_nombre_de_la_cuenta()
    {
        var token = IdToken(new
        {
            iss = "https://login.microsoftonline.com/1234-tenant/v2.0",
            aud = "cliente-ms",
            nonce = "n1",
            preferred_username = "Luis.Rojas@upc.edu.pe",
            name = "Luis Rojas",
            exp = DateTimeOffset.UtcNow.AddMinutes(5).ToUnixTimeSeconds()
        });

        Assert.Equal("luis.rojas@upc.edu.pe", ConexionIdentidad.Validar(Microsoft, token, "n1").Email);
    }

    private static (ConexionIdentidad Conexion, ProveedorIdentidadSimulado Proveedor) Montar(
        BancoDePruebas banco, string[]? administradores = null, string[]? docentes = null, bool registroAbierto = true)
    {
        var opciones = Options.Create(new OpcionesAcceso
        {
            Proveedores = new() { ["google"] = Google },
            Administradores = administradores ?? [],
            Docentes = docentes ?? [],
            RegistroAbierto = registroAbierto,
            ClaveSesion = Convert.ToBase64String(new byte[32])
        });

        var proveedor = new ProveedorIdentidadSimulado();
        var conexion = new ConexionIdentidad(
            new MemoryCache(new MemoryCacheOptions()), proveedor, banco.Db,
            new ServicioSesion(opciones, new HostingEnvironment { EnvironmentName = "Testing" }),
            opciones, NullLogger<ConexionIdentidad>.Instance);

        return (conexion, proveedor);
    }

    private static Dictionary<string, string> Parametros(string url) =>
        new Uri(url).Query.TrimStart('?').Split('&').Select(p => p.Split('=', 2))
            .ToDictionary(p => p[0], p => Uri.UnescapeDataString(p[1]));

    [Fact]
    public async Task Un_usuario_registrado_entra_con_su_rol_de_la_plataforma()
    {
        using var banco = new BancoDePruebas().Sembrar();
        var alumna = banco.Db.Usuarios.Single();
        var (conexion, proveedor) = Montar(banco);

        var url = Parametros(conexion.Iniciar("google"));
        Assert.Equal("S256", url["code_challenge_method"]);
        Assert.Equal("openid email profile", url["scope"]);

        proveedor.IdToken = IdToken(Carga(email: alumna.Email, nonce: url["nonce"]));
        var sesion = await conexion.CanjearAsync("google", "codigo", url["state"], CancellationToken.None);

        Assert.Equal((alumna.Id, "Alumno"), (sesion.Usuario.Id, sesion.Usuario.Rol));
        Assert.Contains("code_verifier=", proveedor.CuerpoRecibido);

        // El estado es de un solo uso.
        var repetido = await Assert.ThrowsAsync<AccesoException>(
            () => conexion.CanjearAsync("google", "codigo", url["state"], CancellationToken.None));
        Assert.Equal("acceso_vencido", repetido.Codigo);
    }

    [Fact]
    public async Task Con_registro_cerrado_quien_no_esta_registrado_no_entra()
    {
        using var banco = new BancoDePruebas().Sembrar();
        var (conexion, proveedor) = Montar(banco, registroAbierto: false);

        var url = Parametros(conexion.Iniciar("google"));
        proveedor.IdToken = IdToken(Carga(email: "intruso@gmail.com", nonce: url["nonce"]));

        var error = await Assert.ThrowsAsync<AccesoException>(
            () => conexion.CanjearAsync("google", "codigo", url["state"], CancellationToken.None));

        Assert.Equal(("no_registrado", 403), (error.Codigo, error.Estado));
        Assert.DoesNotContain(banco.Db.Usuarios, u => u.Email == "intruso@gmail.com");
    }

    [Fact]
    public async Task El_primer_administrador_sale_de_la_configuracion()
    {
        using var banco = new BancoDePruebas().Sembrar();
        var (conexion, proveedor) = Montar(banco, administradores: ["Directora@upc.edu.pe"]);

        var url = Parametros(conexion.Iniciar("google"));
        proveedor.IdToken = IdToken(Carga(email: "directora@upc.edu.pe", nonce: url["nonce"]));

        var sesion = await conexion.CanjearAsync("google", "codigo", url["state"], CancellationToken.None);

        Assert.Equal("Admin", sesion.Usuario.Rol);
        Assert.Equal(RolUsuario.Admin, banco.Db.Usuarios.Single(u => u.Email == "directora@upc.edu.pe").Rol);
    }

    private static async Task<Sesion> EntrarAsync(
        ConexionIdentidad conexion, ProveedorIdentidadSimulado proveedor, string email)
    {
        var url = Parametros(conexion.Iniciar("google"));
        proveedor.IdToken = IdToken(Carga(email: email, nonce: url["nonce"]));
        return await conexion.CanjearAsync("google", "codigo", url["state"], CancellationToken.None);
    }

    [Fact]
    public async Task Con_registro_abierto_cualquiera_entra_como_alumno_de_todos_los_cursos()
    {
        using var banco = new BancoDePruebas().Sembrar();
        var (conexion, proveedor) = Montar(banco);

        var sesion = await EntrarAsync(conexion, proveedor, "cualquiera@gmail.com");

        Assert.Equal("Alumno", sesion.Usuario.Rol);
        var cursos = banco.Db.Cursos.Select(c => c.Id).ToList();
        Assert.NotEmpty(cursos);
        Assert.All(cursos, c => Assert.Contains(banco.Db.Matriculas, m => m.UsuarioId == sesion.Usuario.Id && m.CursoId == c));

        // Un curso nuevo en la carpeta aparece en su siguiente acceso.
        var nuevo = new Curso { Codigo = "MNEW", Nombre = "Curso nuevo" };
        banco.Db.Cursos.Add(nuevo);
        banco.Db.SaveChanges();

        await EntrarAsync(conexion, proveedor, "cualquiera@gmail.com");
        Assert.Contains(banco.Db.Matriculas, m => m.UsuarioId == sesion.Usuario.Id && m.CursoId == nuevo.Id);
        Assert.Single(banco.Db.Usuarios, u => u.Email == "cualquiera@gmail.com");
    }

    [Fact]
    public async Task El_profesor_de_la_configuracion_entra_como_docente_aunque_antes_entrara_como_alumno()
    {
        using var banco = new BancoDePruebas().Sembrar();

        var (abierta, proveedor) = Montar(banco);
        Assert.Equal("Alumno", (await EntrarAsync(abierta, proveedor, "profe@gmail.com")).Usuario.Rol);

        var (conProfesor, proveedor2) = Montar(banco, docentes: ["Profe@gmail.com"]);
        var sesion = await EntrarAsync(conProfesor, proveedor2, "profe@gmail.com");

        Assert.Equal("Docente", sesion.Usuario.Rol);
        Assert.Equal(banco.Db.Cursos.Count(), banco.Db.Matriculas.Count(m => m.UsuarioId == sesion.Usuario.Id));
    }
}

public class SesionApiTests
{
    [Fact]
    public async Task La_sesion_emitida_abre_la_api_y_una_falsificada_no()
    {
        using var api = new ApiDePruebas();
        var (_, alumnoId, _, _) = api.Sembrar();

        var entrada = await api.CreateClient().PostAsJsonAsync("/api/v1/acceso/desarrollo", new { usuarioId = alumnoId });
        var sesion = await entrada.Content.ReadFromJsonAsync<JsonElement>();
        var token = sesion.GetProperty("token").GetString()!;

        var cliente = api.CreateClient();
        cliente.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        var yo = await cliente.GetFromJsonAsync<JsonElement>("/api/v1/acceso/yo");
        Assert.Equal("Alumno", yo.GetProperty("rol").GetString());

        var falsa = api.CreateClient();
        falsa.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AccesoTests.IdToken(new
        {
            sub = alumnoId, role = "Admin", iss = "tutorpreclase", aud = "tutorpreclase",
            exp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds()
        }));

        Assert.Equal(HttpStatusCode.Unauthorized, (await falsa.GetAsync("/api/v1/acceso/yo")).StatusCode);
    }

    [Fact]
    public async Task El_administrador_da_de_alta_y_matricula_y_el_alumno_no_puede()
    {
        using var api = new ApiDePruebas();
        var (_, alumnoId, cursoId, _) = api.Sembrar();
        var admin = api.Como(Guid.NewGuid(), "Admin");

        var alta = await admin.PutAsJsonAsync("/api/v1/admin/usuarios", new
        {
            email = "Nuevo.Alumno@UPC.edu.pe", nombre = "Nuevo Alumno", rol = "Alumno", cursos = new[] { cursoId }
        });
        Assert.Equal(HttpStatusCode.OK, alta.StatusCode);

        using (var db = api.NuevoContexto())
        {
            var nuevo = db.Usuarios.Single(u => u.Email == "nuevo.alumno@upc.edu.pe");
            Assert.Contains(db.Matriculas, m => m.UsuarioId == nuevo.Id && m.CursoId == cursoId);
        }

        var rolInventado = await admin.PutAsJsonAsync("/api/v1/admin/usuarios", new
        {
            email = "x@upc.edu.pe", nombre = "X", rol = "Rector", cursos = Array.Empty<Guid>()
        });
        Assert.Equal(HttpStatusCode.BadRequest, rolInventado.StatusCode);

        Assert.Equal(HttpStatusCode.Forbidden,
            (await api.Como(alumnoId, "Alumno").GetAsync("/api/v1/admin/usuarios")).StatusCode);

        // En esta etapa el profesor hace de administrador.
        Assert.Equal(HttpStatusCode.OK,
            (await api.Como(Guid.NewGuid(), "Docente").GetAsync("/api/v1/admin/usuarios")).StatusCode);
    }
}
