using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TutorPreClase.Application.Llm;

namespace TutorPreClase.Infrastructure.Llm;

/// <summary>
/// Adaptador del formato de chat de OpenAI (function calling, streaming). Es la base de
/// OpenRouter, que lo habla tal cual bajo otra ruta (SDD §5.8).
/// </summary>
public class OpenAiProvider(
    HttpClient http,
    OpcionesAgente opciones,
    ILogger logger,
    string id = "openai") : ILlmProvider
{
    public string Id { get; } = id;

    protected OpcionesAgente Opciones { get; } = opciones;

    /// <summary>Ruta del chat relativa a la BaseUrl; cada compatible la puede mover.</summary>
    protected virtual string RutaChat => "/v1/chat/completions";

    /// <summary>Preferencias de enrutado que algunos compatibles aceptan; null las omite.</summary>
    protected virtual object? PreferenciasProveedor => null;

    /// <summary>Modelos de respaldo que el compatible prueba si falla el principal; null los omite.</summary>
    protected virtual IReadOnlyList<string>? Respaldo(string modelo) => null;

    public async IAsyncEnumerable<LlmEvento> StreamAsync(
        LlmSolicitud solicitud, [EnumeratorCancellation] CancellationToken ct)
    {
        using var peticion = new HttpRequestMessage(HttpMethod.Post, RutaChat)
        {
            Content = JsonContent.Create(Cuerpo(solicitud), options: MapeoLlm.Json)
        };

        if (!string.IsNullOrEmpty(Opciones.ApiKey))
            peticion.Headers.Authorization = new("Bearer", Opciones.ApiKey);

        var envio = await MapeoLlm.EnviarAsync(http, peticion, Id, logger, ct);

        if (envio.Error is not null)
        {
            yield return envio.Error;
            yield break;
        }

        using (var respuesta = envio.Respuesta!)
        {
            var fallo = await MapeoLlm.RevisarEstadoAsync(respuesta, Id, logger, ct);
            if (fallo is not null)
            {
                yield return fallo;
                yield break;
            }

            var entrada = 0;
            var salida = 0;
            string? herramientaId = null;
            string? herramientaNombre = null;
            var argumentos = new StringBuilder();

            await using var flujo = await respuesta.Content.ReadAsStreamAsync(ct);

            await foreach (var dato in MapeoLlm.LeerSseAsync(flujo, ct))
            {
                JsonElement evento;
                try
                {
                    evento = JsonDocument.Parse(dato).RootElement;
                }
                catch (JsonException)
                {
                    continue;
                }

                entrada += MapeoLlm.Entero(evento, "usage", "prompt_tokens");
                salida += MapeoLlm.Entero(evento, "usage", "completion_tokens");

                if (!evento.TryGetProperty("choices", out var opcionesJson) ||
                    opcionesJson.ValueKind != JsonValueKind.Array ||
                    opcionesJson.GetArrayLength() == 0)
                {
                    continue;
                }

                var opcion = opcionesJson[0];
                if (!opcion.TryGetProperty("delta", out var delta)) continue;

                var texto = MapeoLlm.Texto(delta, "content");
                if (texto.Length > 0) yield return new TextoParcial(texto);

                if (delta.TryGetProperty("tool_calls", out var llamadas) &&
                    llamadas.ValueKind == JsonValueKind.Array)
                {
                    foreach (var llamada in llamadas.EnumerateArray())
                    {
                        var idParcial = MapeoLlm.Texto(llamada, "id");
                        if (idParcial.Length > 0) herramientaId = idParcial;

                        var nombre = MapeoLlm.Texto(llamada, "function", "name");
                        if (nombre.Length > 0) herramientaNombre = nombre;

                        argumentos.Append(MapeoLlm.Texto(llamada, "function", "arguments"));
                    }
                }

                var razon = MapeoLlm.Texto(opcion, "finish_reason");
                if (razon == "tool_calls" && herramientaId is not null && herramientaNombre is not null)
                {
                    yield return new LlamadaHerramienta(
                        herramientaId, herramientaNombre, MapeoLlm.Argumentos(argumentos.ToString()));

                    herramientaId = null;
                    herramientaNombre = null;
                    argumentos.Clear();
                }
            }

            yield return new Fin(entrada, salida);
        }
    }

    private object Cuerpo(LlmSolicitud s)
    {
        var mensajes = new List<object> { new { role = "system", content = s.PromptSistema } };

        foreach (var m in s.Mensajes)
        {
            switch (m.Rol)
            {
                case RolLlm.Usuario:
                    mensajes.Add(new { role = "user", content = m.Texto ?? "" });
                    break;

                case RolLlm.Asistente when m.HerramientaId is not null:
                    mensajes.Add(new
                    {
                        role = "assistant",
                        content = string.IsNullOrWhiteSpace(m.Texto) ? null : m.Texto,
                        tool_calls = new object[]
                        {
                            new
                            {
                                id = m.HerramientaId,
                                type = "function",
                                function = new
                                {
                                    name = m.HerramientaNombre,
                                    arguments = (m.HerramientaArgumentos ?? MapeoLlm.ObjetoVacio).GetRawText()
                                }
                            }
                        }
                    });
                    break;

                case RolLlm.Asistente:
                    mensajes.Add(new { role = "assistant", content = m.Texto ?? "" });
                    break;

                case RolLlm.Herramienta:
                    mensajes.Add(new
                    {
                        role = "tool",
                        tool_call_id = m.HerramientaId,
                        content = m.HerramientaResultado ?? ""
                    });
                    break;
            }
        }

        return new
        {
            model = s.Modelo,
            models = Respaldo(s.Modelo),
            max_tokens = s.MaxTokens,
            temperature = s.Temperatura,
            stream = true,
            stream_options = new { include_usage = true },
            provider = PreferenciasProveedor,
            messages = mensajes,
            tools = s.Herramientas.Select(h => new
            {
                type = "function",
                function = new
                {
                    name = h.Nombre,
                    description = h.Descripcion,
                    parameters = h.EsquemaEntrada
                }
            })
        };
    }
}

/// <summary>
/// OpenRouter es compatible con OpenAI bajo `/api/v1`. Sirve los modelos gratuitos
/// (`:free`) que el alumno usa sin pagar, tras iniciar sesion por OAuth (RF-29).
/// </summary>
public sealed class OpenRouterProvider(HttpClient http, OpcionesAgente opciones, ILogger<OpenRouterProvider> logger)
    : OpenAiProvider(http, opciones, logger, "openrouter")
{
    protected override string RutaChat => "/api/v1/chat/completions";

    /// <summary>
    /// Solo proveedores que no guardan ni entrenan con los prompts (RNF-11): lo garantiza
    /// la plataforma en cada llamada, sin que el alumno toque su configuracion de privacidad.
    /// </summary>
    protected override object? PreferenciasProveedor => new { data_collection = "deny" };

    /// <summary>
    /// Si el modelo gratuito está saturado (429 del pool compartido) o caído, OpenRouter
    /// pasa en la misma llamada al siguiente de la lista, con las mismas preferencias.
    /// </summary>
    protected override IReadOnlyList<string>? Respaldo(string modelo)
    {
        var alternativos = Opciones.ModelosAlternativos.Where(m => m != modelo).ToList();
        return alternativos.Count == 0 ? null : [modelo, .. alternativos];
    }
}
