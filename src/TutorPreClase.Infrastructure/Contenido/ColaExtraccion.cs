using System.Threading.Channels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace TutorPreClase.Infrastructure.Contenido;

/// <summary>
/// Cola de extraccion de texto. El SDD contempla RabbitMQ + MassTransit; esta
/// implementacion en proceso cumple el mismo contrato para desplegar sin broker.
/// Cambiar de transporte es sustituir esta clase, sin tocar la API ni el servicio.
/// </summary>
public interface IColaExtraccion
{
    ValueTask EncolarAsync(Guid archivoId, CancellationToken ct = default);
    IAsyncEnumerable<Guid> LeerAsync(CancellationToken ct);
}

public sealed class ColaExtraccionEnMemoria : IColaExtraccion
{
    private readonly Channel<Guid> _canal = Channel.CreateUnbounded<Guid>(new UnboundedChannelOptions
    {
        SingleReader = false,
        SingleWriter = false
    });

    public ValueTask EncolarAsync(Guid archivoId, CancellationToken ct = default) =>
        _canal.Writer.WriteAsync(archivoId, ct);

    public IAsyncEnumerable<Guid> LeerAsync(CancellationToken ct) =>
        _canal.Reader.ReadAllAsync(ct);
}

/// <summary>Consume la cola y extrae el texto de cada archivo subido (SDD §6.1).</summary>
public sealed class ServicioExtraccionEnSegundoPlano(
    IColaExtraccion cola,
    IServiceScopeFactory ambitos,
    ILogger<ServicioExtraccionEnSegundoPlano> log) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        await foreach (var archivoId in cola.LeerAsync(ct))
        {
            try
            {
                using var ambito = ambitos.CreateScope();
                var contenido = ambito.ServiceProvider.GetRequiredService<IServicioContenido>();

                await contenido.ProcesarAsync(archivoId, ct);
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                // El archivo queda en Error con su motivo; la cola no debe caerse por uno.
                log.LogError(ex, "Fallo el procesamiento del archivo {Archivo}", archivoId);
            }
        }
    }
}
