namespace TutorPreClase.Domain.Entidades;

public class Examen
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClaseId { get; set; }
    public Clase? Clase { get; set; }
    public DateTimeOffset AbreEn { get; set; }
    public DateTimeOffset CierraEn { get; set; }
    public int MaxIntentos { get; set; } = 3;
    public int? MinutosLimite { get; set; }
    public ModoFeedback ModoFeedback { get; set; } = ModoFeedback.AlFinal;
    public bool Publicado { get; set; }

    public List<Pregunta> Preguntas { get; set; } = [];
}

public class Pregunta
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ExamenId { get; set; }
    public Examen? Examen { get; set; }
    public string Enunciado { get; set; } = "";
    public string Justificacion { get; set; } = "";
    public string Tema { get; set; } = "";
    public NivelAlumnoValor? Nivel { get; set; }
    public int Orden { get; set; }
    public OrigenPregunta Origen { get; set; } = OrigenPregunta.IA;
    public bool Aprobada { get; set; }

    public List<Alternativa> Alternativas { get; set; } = [];
    public List<PreguntaReferencia> Referencias { get; set; } = [];
}

public class Alternativa
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid PreguntaId { get; set; }
    public Pregunta? Pregunta { get; set; }
    public string Letra { get; set; } = "";
    public string Texto { get; set; } = "";
    public bool EsCorrecta { get; set; }
}

public class PreguntaReferencia
{
    public Guid PreguntaId { get; set; }
    public Pregunta? Pregunta { get; set; }
    public Guid PaginaId { get; set; }
    public PaginaContenido? Pagina { get; set; }
}
