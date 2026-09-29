using TutorPreClase.Domain.Entidades;

namespace TutorPreClase.Application.Llm;

/// <summary>Lo unico que la web puede saber de una credencial (RF-26).</summary>
public sealed record CredencialResumen(
    string AgenteId,
    string Ultimos4,
    OrigenCredencial Origen,
    EstadoCredencial Estado,
    DateTimeOffset CreadaEn,
    DateTimeOffset? UltimoUsoEn);

public sealed class CredencialException(string codigo, string mensaje) : Exception(mensaje)
{
    public string Codigo { get; } = codigo;
}

/// <summary>
/// Guarda y entrega las credenciales BYOK (SDD §3.1). Descifra solo para armar la
/// llamada saliente del alumno dueño; nunca devuelve la clave hacia afuera.
/// </summary>
public interface IBovedaCredenciales
{
    Task<IReadOnlyList<CredencialResumen>> ListarAsync(Guid usuarioId, CancellationToken ct = default);

    /// <summary>Valida contra el proveedor antes de guardar (RF-25).</summary>
    Task<CredencialResumen> ConectarAsync(
        Guid usuarioId,
        string agenteId,
        string clave,
        OrigenCredencial origen = OrigenCredencial.Pegada,
        CancellationToken ct = default);

    Task DesconectarAsync(Guid usuarioId, string agenteId, CancellationToken ct = default);

    /// <summary>Clave en claro para la llamada saliente. Nunca sale de la API.</summary>
    Task<string?> ClaveParaAsync(Guid usuarioId, string agenteId, CancellationToken ct = default);

    Task<IReadOnlyCollection<string>> AgentesConectadosAsync(Guid usuarioId, CancellationToken ct = default);

    /// <summary>El proveedor rechazo la credencial: se marca para que el alumno la reconecte (RF-28).</summary>
    Task MarcarInvalidaAsync(Guid usuarioId, string agenteId, CancellationToken ct = default);
}
