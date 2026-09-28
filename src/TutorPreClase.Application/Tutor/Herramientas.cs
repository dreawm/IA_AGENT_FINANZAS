using System.Text.Json;
using TutorPreClase.Application.Llm;
using TutorPreClase.Domain.Entidades;

namespace TutorPreClase.Application.Tutor;

/// <summary>
/// Catálogo de herramientas del servidor (SDD §5.7). El servidor expone al agente solo
/// las del modo en curso y rechaza cualquier otra, de modo que la seguridad del examen
/// no depende de que el modelo obedezca el prompt.
/// </summary>
public static class Herramientas
{
    public const string ObtenerSiguientePregunta = "obtener_siguiente_pregunta";
    public const string RegistrarRespuesta = "registrar_respuesta";
    public const string ObtenerProgreso = "obtener_progreso";
    public const string FinalizarExamen = "finalizar_examen";
    public const string ObtenerResultado = "obtener_resultado";
    public const string RegistrarDudaSinCobertura = "registrar_duda_sin_cobertura";
    public const string ProponerDiagnostico = "proponer_diagnostico";

    private static readonly Dictionary<ModoConversacion, string[]> PorModo = new()
    {
        [ModoConversacion.Evaluacion] =
        [
            ObtenerSiguientePregunta, RegistrarRespuesta, ObtenerProgreso, FinalizarExamen
        ],
        [ModoConversacion.Revision] =
        [
            ObtenerResultado, RegistrarDudaSinCobertura, ProponerDiagnostico
        ],
        [ModoConversacion.Consulta] =
        [
            RegistrarDudaSinCobertura
        ]
    };

    public static bool Permitida(ModoConversacion modo, string nombre) =>
        PorModo.TryGetValue(modo, out var permitidas) && permitidas.Contains(nombre);

    public static IReadOnlyList<DefinicionHerramienta> Para(ModoConversacion modo) =>
        PorModo[modo].Select(Definir).ToList();

    private static DefinicionHerramienta Definir(string nombre) => nombre switch
    {
        ObtenerSiguientePregunta => new(nombre,
            "Devuelve la siguiente pregunta pendiente del intento, sin la alternativa correcta.",
            Esquema("""{"type":"object","properties":{},"required":[]}""")),

        RegistrarRespuesta => new(nombre,
            "Guarda la alternativa elegida por el alumno. La respuesta es inmutable.",
            Esquema("""
            {"type":"object","properties":{
              "pregunta_id":{"type":"string","description":"Id de la pregunta; omitelo para usar la pregunta en curso"},
              "alternativa":{"type":"string","description":"Letra elegida: A, B, C o D"},
              "texto_original":{"type":"string","description":"Lo que escribió el alumno"}},
             "required":["alternativa"]}
            """)),

        ObtenerProgreso => new(nombre,
            "Respondidas, total y tiempo restante del intento.",
            Esquema("""{"type":"object","properties":{},"required":[]}""")),

        FinalizarExamen => new(nombre,
            "Cierra y califica el intento. Devuelve nota, correctas, total y el detalle de fallos.",
            Esquema("""{"type":"object","properties":{},"required":[]}""")),

        ObtenerResultado => new(nombre,
            "Relee la nota y el detalle de fallos del intento ya enviado.",
            Esquema("""{"type":"object","properties":{},"required":[]}""")),

        RegistrarDudaSinCobertura => new(nombre,
            "Registra una duda que el material de la clase no cubre, para el reporte del docente.",
            Esquema("""
            {"type":"object","properties":{
              "texto":{"type":"string","description":"La duda del alumno"},
              "tema":{"type":"string","description":"Tema al que pertenece"},
              "con_ampliacion":{"type":"boolean","description":"true si respondiste con una ampliación marcada"}},
             "required":["texto"]}
            """)),

        ProponerDiagnostico => new(nombre,
            "Aporta los temas donde viste vacíos. No modifica el nivel del alumno.",
            Esquema("""
            {"type":"object","properties":{
              "temas_debiles":{"type":"array","items":{"type":"string"}}},
             "required":["temas_debiles"]}
            """)),

        _ => throw new ArgumentOutOfRangeException(nameof(nombre), nombre, "Herramienta desconocida.")
    };

    private static JsonElement Esquema(string json) => JsonDocument.Parse(json).RootElement.Clone();
}
