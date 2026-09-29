using System.Buffers.Text;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TutorPreClase.Application.Llm;
using TutorPreClase.Domain.Entidades;

namespace TutorPreClase.Infrastructure.Llm;

/// <summary>
/// OAuth con PKCE de OpenRouter (RF-29, SDD §5.1). El verificador vive en el servidor,
/// ligado al alumno autenticado y de un solo uso: un codigo robado o ajeno no se puede
/// canjear sin el. El canje lo hace la API, asi que la clave nunca pasa por el navegador.
/// </summary>
public sealed class ConexionOpenRouter(
    IMemoryCache cache,
    IHttpClientFactory http,
    IBovedaCredenciales boveda,
    IOptions<OpcionesAgentes> agentes,
    ILogger<ConexionOpenRouter> log) : IConexionOAuth
{
    /// <summary>OpenRouter da 10 minutos para canjear el codigo; el verificador, lo mismo.</summary>
    public static readonly TimeSpan Vigencia = TimeSpan.FromMinutes(10);

    public Task<string> IniciarAsync(Guid usuarioId, string agenteId, CancellationToken ct = default)
    {
        var opciones = OpcionesOAuth(agenteId);

        var verificador = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(32));
        var desafio = Base64Url.EncodeToString(SHA256.HashData(Encoding.ASCII.GetBytes(verificador)));

        // Uno por alumno y agente: volver a iniciar invalida el anterior.
        cache.Set(ClaveCache(usuarioId, agenteId), verificador, Vigencia);

        var url = $"{opciones.UrlAutorizacion}" +
                  $"?callback_url={Uri.EscapeDataString(opciones.UrlRetorno!)}" +
                  $"&code_challenge={desafio}" +
                  "&code_challenge_method=S256" +
                  $"&key_label={Uri.EscapeDataString("Tutor Pre-Clase")}";

        return Task.FromResult(url);
    }

    public async Task<CredencialResumen> CanjearAsync(
        Guid usuarioId, string agenteId, string codigo, CancellationToken ct = default)
    {
        OpcionesOAuth(agenteId);

        if (string.IsNullOrWhiteSpace(codigo))
            throw new CredencialException("oauth_sin_codigo", "OpenRouter no devolvió ningún código de autorización.");

        // Se retira antes de canjear, salga bien o mal: el verificador es de un solo uso.
        var claveCache = ClaveCache(usuarioId, agenteId);
        if (!cache.TryGetValue(claveCache, out string? verificador) || verificador is null)
            throw new CredencialException("oauth_vencido",
                "La conexión con OpenRouter venció o ya se usó. Vuelve a pulsar «Entrar con OpenRouter».");

        cache.Remove(claveCache);

        var cliente = http.CreateClient($"llm:{agenteId}");
        var clave = await CanjearCodigoAsync(cliente, codigo.Trim(), verificador, ct);

        // Desde aqui es una credencial mas: se valida, se cifra y se guarda (RF-25, RF-26).
        return await boveda.ConectarAsync(usuarioId, agenteId, clave, OrigenCredencial.OAuth, ct);
    }

    private async Task<string> CanjearCodigoAsync(
        HttpClient cliente, string codigo, string verificador, CancellationToken ct)
    {
        HttpResponseMessage respuesta;
        try
        {
            respuesta = await cliente.PostAsJsonAsync(
                "/api/v1/auth/keys",
                new { code = codigo, code_verifier = verificador, code_challenge_method = "S256" },
                ct);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException && !ct.IsCancellationRequested)
        {
            log.LogWarning(ex, "OpenRouter no respondio al canje OAuth");
            throw new CredencialException("oauth_sin_respuesta",
                "No se pudo contactar con OpenRouter. Inténtalo de nuevo en un momento.");
        }

        using (respuesta)
        {
            // El cuerpo no se registra: si el canje sale bien, trae la clave.
            if (!respuesta.IsSuccessStatusCode)
            {
                log.LogWarning("OpenRouter rechazo el canje OAuth con {Codigo}", (int)respuesta.StatusCode);
                throw new CredencialException("oauth_rechazado",
                    "OpenRouter no aceptó la autorización. Vuelve a pulsar «Entrar con OpenRouter».");
            }

            var cuerpo = await respuesta.Content.ReadFromJsonAsync<RespuestaCanje>(ct);

            if (string.IsNullOrWhiteSpace(cuerpo?.Key))
                throw new CredencialException("oauth_rechazado", "OpenRouter no devolvió ninguna credencial.");

            return cuerpo.Key;
        }
    }

    private OpcionesAgente OpcionesOAuth(string agenteId)
    {
        if (!agentes.Value.TryGetValue(agenteId, out var opciones) ||
            opciones.Conexion != ConexionAgente.OAuth ||
            string.IsNullOrWhiteSpace(opciones.UrlAutorizacion) ||
            string.IsNullOrWhiteSpace(opciones.UrlRetorno))
        {
            throw new CredencialException("oauth_no_disponible", "Ese agente no se conecta iniciando sesión.");
        }

        return opciones;
    }

    private static string ClaveCache(Guid usuarioId, string agenteId) => $"oauth:{agenteId}:{usuarioId}";

    private sealed record RespuestaCanje(string? Key);
}
