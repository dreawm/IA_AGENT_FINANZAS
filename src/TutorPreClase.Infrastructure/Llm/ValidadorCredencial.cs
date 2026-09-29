using System.Net;
using Microsoft.Extensions.Logging;
using TutorPreClase.Domain.Entidades;

namespace TutorPreClase.Infrastructure.Llm;

/// <summary>Comprueba contra el proveedor que la credencial sirve, antes de guardarla (RF-25).</summary>
public interface IValidadorCredencial
{
    Task<bool> EsUsableAsync(AgenteIA agente, string clave, CancellationToken ct = default);
}

public sealed class ValidadorCredencial(
    IHttpClientFactory http,
    ILogger<ValidadorCredencial> log) : IValidadorCredencial
{
    public async Task<bool> EsUsableAsync(AgenteIA agente, string clave, CancellationToken ct = default)
    {
        var cliente = http.CreateClient($"llm:{agente.Id}");
        cliente.Timeout = TimeSpan.FromSeconds(20);

        using var peticion = Armar(agente, clave);

        try
        {
            using var respuesta = await cliente.SendAsync(peticion, ct);

            // Un 401/403 es credencial mala; un 429 o un 5xx es del proveedor, no de la clave.
            if (respuesta.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                return false;

            if (respuesta.IsSuccessStatusCode) return true;

            log.LogWarning("Validacion de {Agente} devolvio {Codigo}; se acepta la credencial",
                agente.Id, (int)respuesta.StatusCode);

            return true;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // Sin red no se puede afirmar que la clave sea mala: se deja pasar y ya fallara al usarla.
            log.LogWarning(ex, "No se pudo validar la credencial de {Agente}", agente.Id);
            return true;
        }
    }

    /// <summary>
    /// Consultar la propia clave en OpenRouter autentica sin gastar el cupo gratuito.
    /// </summary>
    private static HttpRequestMessage Armar(AgenteIA agente, string clave)
    {
        var peticion = new HttpRequestMessage(HttpMethod.Get, "/api/v1/key");
        peticion.Headers.Authorization = new("Bearer", clave);
        return peticion;
    }
}
