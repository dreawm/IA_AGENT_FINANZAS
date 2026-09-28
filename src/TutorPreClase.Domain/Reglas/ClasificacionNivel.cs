using TutorPreClase.Domain.Entidades;

namespace TutorPreClase.Domain.Reglas;

/// <summary>
/// Nivel del alumno por rangos de nota (SDD §5.6). Es determinista y lo calcula el
/// servidor: el tutor solo aporta los temas débiles.
/// </summary>
public static class ClasificacionNivel
{
    public static NivelAlumnoValor Desde(decimal notaVigente) => notaVigente switch
    {
        < 0m or > 20m => throw new ArgumentOutOfRangeException(nameof(notaVigente), "La nota debe estar entre 0 y 20."),
        < 11m => NivelAlumnoValor.Inicial,
        < 15m => NivelAlumnoValor.Basico,
        < 18m => NivelAlumnoValor.Intermedio,
        _ => NivelAlumnoValor.Avanzado
    };

    /// <summary>El nivel se calcula sobre el mejor intento enviado del alumno.</summary>
    public static decimal NotaVigente(IEnumerable<Intento> intentos)
    {
        var notas = intentos
            .Where(i => i.Estado != EstadoIntento.EnCurso && i.Puntaje.HasValue)
            .Select(i => i.Puntaje!.Value)
            .ToList();

        return notas.Count == 0 ? 0m : notas.Max();
    }
}
