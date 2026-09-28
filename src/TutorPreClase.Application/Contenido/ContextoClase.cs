using System.Text;
using TutorPreClase.Domain.Entidades;

namespace TutorPreClase.Application.Contenido;

/// <summary>Contexto de clase armado (SDD §6.2), listo para el prompt de sistema.</summary>
public sealed record ContextoClase(
    string Texto,
    int Tokens,
    bool Truncado,
    IReadOnlySet<(string Archivo, int Pagina)> PaginasIncluidas)
{
    public static ContextoClase Vacio { get; } =
        new("", 0, false, new HashSet<(string, int)>());
}

public sealed class OpcionesContextoClase
{
    public const string Seccion = "ContextoClase";

    /// <summary>Tope duro del contexto de clase (SDD §6.1).</summary>
    public int TokensMaximos { get; set; } = 150_000;

    /// <summary>A partir de aquí se avisa al docente en la pantalla de la clase.</summary>
    public int TokensAviso { get; set; } = 120_000;
}

/// <summary>
/// Arma el bloque &lt;contenido&gt; concatenando las páginas de la clase en orden de
/// archivo y página, cada una con su marca [archivo, p. N] — la misma cita que debe
/// usar el agente. No hay búsqueda ni recuperación: va el material completo.
/// </summary>
public static class ArmadorContextoClase
{
    public static ContextoClase Armar(
        string tituloClase,
        IEnumerable<ArchivoContenido> archivos,
        OpcionesContextoClase opciones)
    {
        var paginas = archivos
            .Where(a => a.Estado == EstadoArchivo.Listo)
            .SelectMany(a => a.Paginas.Select(p => (Archivo: a.Nombre, p.Pagina, p.Texto, p.Tokens)))
            .OrderBy(p => p.Archivo, StringComparer.OrdinalIgnoreCase)
            .ThenBy(p => p.Pagina)
            .ToList();

        if (paginas.Count == 0) return ContextoClase.Vacio;

        var sb = new StringBuilder();
        sb.Append("<contenido clase=\"").Append(Escapar(tituloClase)).Append("\">\n");

        var incluidas = new HashSet<(string, int)>();
        var tokens = 0;
        var truncado = false;

        foreach (var p in paginas)
        {
            if (tokens + p.Tokens > opciones.TokensMaximos)
            {
                truncado = true;
                break;
            }

            sb.Append('[').Append(p.Archivo).Append(", p. ").Append(p.Pagina).Append("]\n");
            sb.Append(p.Texto.Trim()).Append("\n\n");

            incluidas.Add((p.Archivo, p.Pagina));
            tokens += p.Tokens;
        }

        sb.Append("</contenido>");
        return new ContextoClase(sb.ToString(), tokens, truncado, incluidas);
    }

    private static string Escapar(string valor) => valor.Replace("\"", "'");
}
