using Microsoft.EntityFrameworkCore;
using TutorPreClase.Domain.Entidades;

namespace TutorPreClase.Infrastructure.Persistencia;

/// <summary>
/// Agente de fabrica (SDD §5.8): OpenRouter, la unica via para que el alumno conecte su
/// cuenta. Se agrega si falta, sin tocar el existente: el administrador puede haber
/// cambiado su modelo.
/// </summary>
public static class DatosIniciales
{
    public static async Task SembrarAsync(AppDbContext db, CancellationToken ct = default)
    {
        var existentes = await db.Agentes.Select(a => a.Id).ToListAsync(ct);
        var faltantes = Fabrica().Where(a => !existentes.Contains(a.Id)).ToList();

        if (faltantes.Count == 0) return;

        db.Agentes.AddRange(faltantes);
        await db.SaveChangesAsync(ct);
    }

    private static IEnumerable<AgenteIA> Fabrica() =>
    [
        // Sin consola: la clave llega por OAuth al iniciar sesion (RF-24, RF-29).
        new AgenteIA
        {
            Id = "openrouter",
            NombreVisible = "OpenRouter (gratis)",
            Proveedor = "OpenRouter",
            Modelo = "qwen/qwen3.8-27b:free",
            BaseUrl = "https://openrouter.ai",
            Descripcion = "Sin costo: entra con tu cuenta de OpenRouter. Tiene un límite de mensajes por día.",
            Habilitado = true
        }
    ];
}
