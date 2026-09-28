namespace TutorPreClase.Domain.Reglas;

/// <summary>
/// Calificación determinista del servidor (SDD §5.3). El agente nunca calcula la nota.
/// </summary>
public static class Calificacion
{
    /// <summary>nota = round(20 * correctas / total, 1) en escala peruana 0–20.</summary>
    public static decimal Nota(int correctas, int total)
    {
        if (total <= 0) return 0m;
        if (correctas < 0 || correctas > total)
            throw new ArgumentOutOfRangeException(nameof(correctas), "Correctas fuera del rango del examen.");

        return Math.Round(20m * correctas / total, 1, MidpointRounding.AwayFromZero);
    }

    public static int Porcentaje(int correctas, int total)
    {
        if (total <= 0) return 0;
        return (int)Math.Round(100m * correctas / total, 0, MidpointRounding.AwayFromZero);
    }
}
