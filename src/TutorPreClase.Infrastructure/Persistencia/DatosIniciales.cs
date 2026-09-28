using Microsoft.EntityFrameworkCore;
using TutorPreClase.Domain.Entidades;

namespace TutorPreClase.Infrastructure.Persistencia;

/// <summary>Agentes habilitados de fabrica (SDD §5.8).</summary>
public static class DatosIniciales
{
    public static async Task SembrarAsync(AppDbContext db, CancellationToken ct = default)
    {
        if (await db.Agentes.AnyAsync(ct)) return;

        db.Agentes.AddRange(
            new AgenteIA
            {
                Id = "claude",
                NombreVisible = "Claude",
                Proveedor = "Anthropic",
                Modelo = "claude-sonnet-5",
                BaseUrl = "https://api.anthropic.com",
                Descripcion = "Explica paso a paso y cita el material de la clase.",
                UrlConsola = "https://console.anthropic.com/settings/keys",
                Habilitado = true
            },
            new AgenteIA
            {
                Id = "openai",
                NombreVisible = "ChatGPT",
                Proveedor = "OpenAI",
                Modelo = "gpt-4.1",
                BaseUrl = "https://api.openai.com",
                Descripcion = "Respuestas directas y ejemplos breves.",
                UrlConsola = "https://platform.openai.com/api-keys",
                Habilitado = true
            },
            new AgenteIA
            {
                Id = "kimi",
                NombreVisible = "Kimi",
                Proveedor = "Moonshot",
                Modelo = "kimi-k2",
                BaseUrl = "https://api.moonshot.ai",
                Descripcion = "Buen manejo de textos largos.",
                UrlConsola = "https://platform.moonshot.ai/console/api-keys",
                Habilitado = true
            });

        await db.SaveChangesAsync(ct);
    }
}
