using Microsoft.EntityFrameworkCore;
using TutorPreClase.Application.Abstracciones;
using TutorPreClase.Application.Nivel;
using TutorPreClase.Domain.Entidades;

namespace TutorPreClase.Application.Reportes;

public sealed record NivelDeAlumno(Guid AlumnoId, string Nombre, string Nivel, IReadOnlyList<string> TemasDebiles, string Origen);

public sealed record PreguntaFallada(Guid PreguntaId, string Enunciado, string Tema, int Fallos, string AlternativaMasElegida);

public sealed record DudaReportada(string Texto, string? Tema, bool ConAmpliacion, DateTimeOffset CreadoEn);

public sealed record UsoAgente(string AgenteId, int Intentos);

public sealed record ReporteClase(
    Guid ClaseId,
    string Clase,
    int Alumnos,
    decimal PromedioNota,
    IReadOnlyDictionary<string, int> DistribucionNotas,
    IReadOnlyDictionary<string, int> DistribucionNiveles,
    IReadOnlyList<NivelDeAlumno> Niveles,
    IReadOnlyList<PreguntaFallada> PreguntasMasFalladas,
    IReadOnlyList<string> TemasDebilesDelGrupo,
    IReadOnlyList<DudaReportada> DudasFueraDelMaterial,
    IReadOnlyList<UsoAgente> UsoPorAgente);

public interface IServicioReporte
{
    Task<ReporteClase?> DeClaseAsync(Guid claseId, CancellationToken ct = default);
}

/// <summary>Reporte del docente (RF-16, RF-22): notas, niveles, temas debiles y dudas.</summary>
public sealed class ServicioReporte(IAppDbContext db) : IServicioReporte
{
    public async Task<ReporteClase?> DeClaseAsync(Guid claseId, CancellationToken ct = default)
    {
        var clase = await db.Clases.AsNoTracking().FirstOrDefaultAsync(c => c.Id == claseId, ct);
        if (clase is null) return null;

        var examen = await db.Examenes
            .AsNoTracking()
            .Include(e => e.Preguntas).ThenInclude(p => p.Alternativas)
            .FirstOrDefaultAsync(e => e.ClaseId == claseId, ct);

        var intentos = examen is null
            ? []
            : await db.Intentos
                .AsNoTracking()
                .Where(i => i.ExamenId == examen.Id && i.Estado != EstadoIntento.EnCurso)
                .Include(i => i.Respuestas)
                .ToListAsync(ct);

        var notas = intentos.Where(i => i.Puntaje.HasValue).Select(i => i.Puntaje!.Value).ToList();

        var niveles = await db.Niveles.AsNoTracking().Where(n => n.ClaseId == claseId).ToListAsync(ct);
        var alumnosIds = niveles.Select(n => n.AlumnoId).ToList();

        var nombres = await db.Usuarios
            .AsNoTracking()
            .Where(u => alumnosIds.Contains(u.Id))
            .ToDictionaryAsync(u => u.Id, u => u.Nombre, ct);

        var nivelesDto = niveles
            .Select(n => new NivelDeAlumno(
                n.AlumnoId,
                nombres.GetValueOrDefault(n.AlumnoId, "(sin nombre)"),
                n.Nivel.ToString(),
                NivelService.Deserializar(n.TemasDebiles),
                n.Origen.ToString()))
            .OrderBy(n => n.Nombre)
            .ToList();

        var dudas = await db.DudasSinCobertura
            .AsNoTracking()
            .Where(d => d.ClaseId == claseId)
            .OrderByDescending(d => d.CreadoEn)
            .Take(100)
            .ToListAsync(ct);

        return new ReporteClase(
            claseId,
            clase.Titulo,
            Alumnos: intentos.Select(i => i.AlumnoId).Distinct().Count(),
            PromedioNota: notas.Count == 0 ? 0m : Math.Round(notas.Average(), 1),
            DistribucionNotas: Histograma(notas),
            DistribucionNiveles: niveles
                .GroupBy(n => n.Nivel.ToString())
                .ToDictionary(g => g.Key, g => g.Count()),
            Niveles: nivelesDto,
            PreguntasMasFalladas: PreguntasFalladas(examen, intentos),
            TemasDebilesDelGrupo: nivelesDto
                .SelectMany(n => n.TemasDebiles)
                .GroupBy(t => t, StringComparer.OrdinalIgnoreCase)
                .OrderByDescending(g => g.Count())
                .Select(g => g.Key)
                .Take(10)
                .ToList(),
            DudasFueraDelMaterial: dudas
                .Select(d => new DudaReportada(d.Texto, d.Tema, d.RespondidaConAmpliacion, d.CreadoEn))
                .ToList(),
            UsoPorAgente: intentos
                .GroupBy(i => i.AgenteId)
                .Select(g => new UsoAgente(g.Key, g.Count()))
                .OrderByDescending(u => u.Intentos)
                .ToList());
    }

    private static Dictionary<string, int> Histograma(List<decimal> notas)
    {
        var tramos = new Dictionary<string, int>
        {
            ["0-10"] = 0, ["11-14"] = 0, ["15-17"] = 0, ["18-20"] = 0
        };

        foreach (var nota in notas)
        {
            var tramo = nota switch
            {
                < 11m => "0-10",
                < 15m => "11-14",
                < 18m => "15-17",
                _ => "18-20"
            };
            tramos[tramo]++;
        }

        return tramos;
    }

    private static List<PreguntaFallada> PreguntasFalladas(Examen? examen, List<Intento> intentos)
    {
        if (examen is null) return [];

        var fallosPorPregunta = intentos
            .SelectMany(i => i.Respuestas)
            .Where(r => !r.EsCorrecta)
            .GroupBy(r => r.PreguntaId);

        var resultado = new List<PreguntaFallada>();

        foreach (var grupo in fallosPorPregunta)
        {
            var pregunta = examen.Preguntas.FirstOrDefault(p => p.Id == grupo.Key);
            if (pregunta is null) continue;

            var masElegida = grupo
                .GroupBy(r => r.AlternativaId)
                .OrderByDescending(g => g.Count())
                .First().Key;

            var alternativa = pregunta.Alternativas.FirstOrDefault(a => a.Id == masElegida);

            resultado.Add(new PreguntaFallada(
                pregunta.Id,
                pregunta.Enunciado,
                pregunta.Tema,
                grupo.Count(),
                alternativa is null ? "(desconocida)" : alternativa.Letra + ". " + alternativa.Texto));
        }

        return resultado.OrderByDescending(p => p.Fallos).ToList();
    }
}
