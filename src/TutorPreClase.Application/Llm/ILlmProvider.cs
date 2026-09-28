using System.Text.Json;

namespace TutorPreClase.Application.Llm;

/// <summary>
/// Contrato común de los proveedores (SDD §5.8). El resto del sistema no sabe con
/// cuál está hablando: el prompt, las herramientas y el historial son los mismos.
/// </summary>
public interface ILlmProvider
{
    /// <summary>"claude" | "openai" | "kimi".</summary>
    string Id { get; }

    IAsyncEnumerable<LlmEvento> StreamAsync(LlmSolicitud solicitud, CancellationToken ct);
}

public sealed record LlmSolicitud(
    string Modelo,
    string PromptSistema,
    IReadOnlyList<LlmMensaje> Mensajes,
    IReadOnlyList<DefinicionHerramienta> Herramientas,
    double Temperatura = 0.2,
    int MaxTokens = 1024);

public enum RolLlm { Usuario, Asistente, Herramienta }

public sealed record LlmMensaje(
    RolLlm Rol,
    string? Texto,
    string? HerramientaId = null,
    string? HerramientaNombre = null,
    string? HerramientaResultado = null,
    JsonElement? HerramientaArgumentos = null);

public sealed record DefinicionHerramienta(string Nombre, string Descripcion, JsonElement EsquemaEntrada);

public abstract record LlmEvento;
public sealed record TextoParcial(string Texto) : LlmEvento;
public sealed record LlamadaHerramienta(string Id, string Nombre, JsonElement Argumentos) : LlmEvento;
public sealed record Fin(int TokensEntrada, int TokensSalida) : LlmEvento;
public sealed record ErrorProveedor(string Mensaje, bool Reintentable, bool CredencialRechazada = false) : LlmEvento;
