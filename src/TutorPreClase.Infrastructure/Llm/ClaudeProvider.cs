using System.Net.Http.Json;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using TutorPreClase.Application.Llm;

namespace TutorPreClase.Infrastructure.Llm;

/// <summary>
/// Adaptador de la Messages API de Anthropic con tool use y streaming SSE (SDD §5.8).
/// Mapea tool_use / tool_result; el prompt y las herramientas son los comunes.
/// </summary>
public sealed class ClaudeProvider(
    HttpClient http,
    OpcionesAgente opciones,
    ILogger<ClaudeProvider> log) : ILlmProvider
{
    public string Id => "claude";

    public async IAsyncEnumerable<LlmEvento> StreamAsync(
        LlmSolicitud solicitud, [EnumeratorCancellation] CancellationToken ct)
    {
        var cuerpo = Cuerpo(solicitud);

        using var peticion = new HttpRequestMessage(HttpMethod.Post, "/v1/messages")
        {
            Content = JsonContent.Create(cuerpo, options: MapeoLlm.Json)
        };
        peticion.Headers.Add("anthropic-version", "2023-06-01");
        if (!string.IsNullOrEmpty(opciones.ApiKey)) peticion.Headers.Add("x-api-key", opciones.ApiKey);

        var envio = await MapeoLlm.EnviarAsync(http, peticion, Id, log, ct);

        if (envio.Error is not null)
        {
            yield return envio.Error;
            yield break;
        }

        using (var respuesta = envio.Respuesta!)
        {
            var fallo = await MapeoLlm.RevisarEstadoAsync(respuesta, Id, log, ct);
            if (fallo is not null)
            {
                yield return fallo;
                yield break;
            }

            var entrada = 0;
            var salida = 0;
            string? herramientaId = null;
            string? herramientaNombre = null;
            var argumentos = new System.Text.StringBuilder();

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

                var tipo = MapeoLlm.Texto(evento, "type");

                switch (tipo)
                {
                    case "message_start":
                        entrada += MapeoLlm.Entero(evento, "message", "usage", "input_tokens");
                        break;

                    case "content_block_start":
                        if (MapeoLlm.Texto(evento, "content_block", "type") == "tool_use")
                        {
                            herramientaId = MapeoLlm.Texto(evento, "content_block", "id");
                            herramientaNombre = MapeoLlm.Texto(evento, "content_block", "name");
                            argumentos.Clear();
                        }
                        break;

                    case "content_block_delta":
                        var delta = MapeoLlm.Texto(evento, "delta", "type");
                        if (delta == "text_delta")
                        {
                            var texto = MapeoLlm.Texto(evento, "delta", "text");
                            if (texto.Length > 0) yield return new TextoParcial(texto);
                        }
                        else if (delta == "input_json_delta")
                        {
                            argumentos.Append(MapeoLlm.Texto(evento, "delta", "partial_json"));
                        }
                        break;

                    case "content_block_stop":
                        if (herramientaId is not null && herramientaNombre is not null)
                        {
                            yield return new LlamadaHerramienta(
                                herramientaId, herramientaNombre, MapeoLlm.Argumentos(argumentos.ToString()));
                            herramientaId = null;
                            herramientaNombre = null;
                        }
                        break;

                    case "message_delta":
                        salida += MapeoLlm.Entero(evento, "usage", "output_tokens");
                        break;
                }
            }

            yield return new Fin(entrada, salida);
        }
    }

    private static object Cuerpo(LlmSolicitud s)
    {
        var mensajes = new List<object>();

        foreach (var m in s.Mensajes)
        {
            switch (m.Rol)
            {
                case RolLlm.Usuario:
                    mensajes.Add(new { role = "user", content = m.Texto ?? "" });
                    break;

                case RolLlm.Asistente when m.HerramientaId is not null:
                    var bloques = new List<object>();
                    if (!string.IsNullOrWhiteSpace(m.Texto))
                        bloques.Add(new { type = "text", text = m.Texto });

                    bloques.Add(new
                    {
                        type = "tool_use",
                        id = m.HerramientaId,
                        name = m.HerramientaNombre,
                        input = m.HerramientaArgumentos ?? MapeoLlm.ObjetoVacio
                    });

                    mensajes.Add(new { role = "assistant", content = bloques });
                    break;

                case RolLlm.Asistente:
                    mensajes.Add(new { role = "assistant", content = m.Texto ?? "" });
                    break;

                case RolLlm.Herramienta:
                    mensajes.Add(new
                    {
                        role = "user",
                        content = new object[]
                        {
                            new
                            {
                                type = "tool_result",
                                tool_use_id = m.HerramientaId,
                                content = m.HerramientaResultado ?? ""
                            }
                        }
                    });
                    break;
            }
        }

        return new
        {
            model = s.Modelo,
            max_tokens = s.MaxTokens,
            temperature = s.Temperatura,
            stream = true,
            // El contexto de clase va al inicio del prompt de sistema y se cachea (SDD §6.2).
            system = new object[]
            {
                new
                {
                    type = "text",
                    text = s.PromptSistema,
                    cache_control = new { type = "ephemeral" }
                }
            },
            messages = mensajes,
            tools = s.Herramientas.Select(h => new
            {
                name = h.Nombre,
                description = h.Descripcion,
                input_schema = h.EsquemaEntrada
            })
        };
    }
}
