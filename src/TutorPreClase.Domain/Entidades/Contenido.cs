namespace TutorPreClase.Domain.Entidades;

public class ArchivoContenido
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ClaseId { get; set; }
    public Clase? Clase { get; set; }
    public string Nombre { get; set; } = "";
    public string Tipo { get; set; } = "";
    public string BlobUrl { get; set; } = "";
    public string HashSha256 { get; set; } = "";
    public EstadoArchivo Estado { get; set; } = EstadoArchivo.Pendiente;
    public string? Error { get; set; }
    public DateTimeOffset CreadoEn { get; set; } = DateTimeOffset.UtcNow;

    public List<PaginaContenido> Paginas { get; set; } = [];
}

/// <summary>
/// Una página (o diapositiva) de texto extraído. El contexto de clase se arma
/// concatenando estas páginas en orden (SDD §6.2); no hay chunks ni embeddings.
/// </summary>
public class PaginaContenido
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ArchivoId { get; set; }
    public ArchivoContenido? Archivo { get; set; }
    public Guid ClaseId { get; set; }
    public int Pagina { get; set; }
    public string Texto { get; set; } = "";
    public int Tokens { get; set; }
}
