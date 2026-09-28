using Microsoft.EntityFrameworkCore;
using TutorPreClase.Domain.Entidades;
using TutorPreClase.Infrastructure.Contenido;
using TutorPreClase.Infrastructure.Persistencia;

namespace TutorPreClase.Worker;

/// <summary>
/// Worker de extraccion (SDD §3). Toma los archivos que quedaron en Pendiente — porque
/// la API se reinicio o porque se escala la extraccion aparte — y los procesa.
/// </summary>
public sealed class ReprocesadorPendientes(
    IServiceScopeFactory ambitos,
    ILogger<ReprocesadorPendientes> log) : BackgroundService
{
    private static readonly TimeSpan Intervalo = TimeSpan.FromSeconds(15);

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        log.LogInformation("Worker de extraccion iniciado");

        using var temporizador = new PeriodicTimer(Intervalo);

        while (!ct.IsCancellationRequested)
        {
            try
            {
                await ProcesarLoteAsync(ct);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                log.LogError(ex, "Fallo el lote de extraccion; se reintenta en el siguiente ciclo");
            }

            if (!await temporizador.WaitForNextTickAsync(ct)) break;
        }
    }

    private async Task ProcesarLoteAsync(CancellationToken ct)
    {
        using var ambito = ambitos.CreateScope();
        var db = ambito.ServiceProvider.GetRequiredService<AppDbContext>();
        var contenido = ambito.ServiceProvider.GetRequiredService<IServicioContenido>();

        var pendientes = await db.Archivos
            .AsNoTracking()
            .Where(a => a.Estado == EstadoArchivo.Pendiente)
            .OrderBy(a => a.CreadoEn)
            .Select(a => a.Id)
            .Take(20)
            .ToListAsync(ct);

        foreach (var archivoId in pendientes)
        {
            log.LogInformation("Extrayendo texto del archivo {Archivo}", archivoId);
            await contenido.ProcesarAsync(archivoId, ct);
        }
    }
}
