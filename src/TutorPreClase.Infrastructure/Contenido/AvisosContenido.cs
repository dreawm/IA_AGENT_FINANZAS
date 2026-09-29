using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace TutorPreClase.Infrastructure.Contenido;

/// <summary>El docente cambio el material de estas clases del curso.</summary>
public sealed record CambioContenido(Guid CursoId, IReadOnlyList<Guid> Clases);

/// <summary>
/// Avisa a quien escuche (la web de los alumnos) que el docente subio o quito material,
/// para que la pagina se actualice solo cuando de verdad hay algo nuevo (SDD §6.1).
/// </summary>
public interface IAvisosContenido
{
    void Publicar(CambioContenido cambio);
    IAsyncEnumerable<CambioContenido> EscucharAsync(CancellationToken ct);
}

/// <summary>
/// En proceso, como la cola de extraccion: basta con una instancia de la API. Con varias,
/// se sustituye por un bus (Redis, RabbitMQ) sin tocar a quien publica ni a quien escucha.
/// </summary>
public sealed class AvisosContenidoEnMemoria : IAvisosContenido
{
    private readonly ConcurrentDictionary<Guid, Channel<CambioContenido>> _oyentes = new();

    public void Publicar(CambioContenido cambio)
    {
        foreach (var canal in _oyentes.Values)
            canal.Writer.TryWrite(cambio);
    }

    public async IAsyncEnumerable<CambioContenido> EscucharAsync([EnumeratorCancellation] CancellationToken ct)
    {
        // Un oyente lento no frena a los demas: se queda con los avisos mas recientes.
        var canal = Channel.CreateBounded<CambioContenido>(
            new BoundedChannelOptions(16) { FullMode = BoundedChannelFullMode.DropOldest });

        var id = Guid.NewGuid();
        _oyentes[id] = canal;

        try
        {
            await foreach (var cambio in canal.Reader.ReadAllAsync(ct))
                yield return cambio;
        }
        finally
        {
            _oyentes.TryRemove(id, out _);
        }
    }
}
