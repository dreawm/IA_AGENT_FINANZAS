namespace TutorPreClase.Application.Evaluacion;

public sealed record AlternativaDto(Guid Id, string Letra, string Texto);

/// <summary>Pregunta tal como la ve el agente: sin la alternativa correcta.</summary>
public sealed record PreguntaDto(
    Guid Id,
    string Enunciado,
    int Numero,
    int Total,
    IReadOnlyList<AlternativaDto> Alternativas);

public sealed record ProgresoDto(int Respondidas, int Total, int? SegundosRestantes);

public sealed record FalloDto(
    Guid PreguntaId,
    string Enunciado,
    string AlternativaElegida,
    string AlternativaCorrecta,
    string Justificacion,
    string Tema);

public sealed record ResultadoDto(
    Guid IntentoId,
    decimal Nota,
    int Porcentaje,
    int Correctas,
    int Total,
    IReadOnlyList<FalloDto> Fallos);

public sealed record RegistroRespuestaDto(
    bool Registrada,
    bool? EsCorrecta,
    string? Justificacion,
    bool QuedanPreguntas);

/// <summary>Error de negocio; se devuelve al agente como resultado de la herramienta.</summary>
public sealed class ExamenException(string codigo, string mensaje) : Exception(mensaje)
{
    public string Codigo { get; } = codigo;
}
