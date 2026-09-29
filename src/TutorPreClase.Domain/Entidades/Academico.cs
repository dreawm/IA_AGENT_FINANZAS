namespace TutorPreClase.Domain.Entidades;

public class Usuario
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = "";
    public string Nombre { get; set; } = "";
    public RolUsuario Rol { get; set; }
    public string? AgentePreferido { get; set; }

    /// <summary>Solo alumnos: el profesor que eligió; ve únicamente los cursos de ese profesor.</summary>
    public Guid? ProfesorId { get; set; }

    public List<Matricula> Matriculas { get; set; } = [];
}

public class Curso
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Periodo { get; set; } = "";

    /// <summary>Profesor dueño del curso: el de la carpeta <c>course-content/{correo}/</c>.</summary>
    public Guid? DocenteId { get; set; }

    public List<Clase> Clases { get; set; } = [];
    public List<Matricula> Matriculas { get; set; } = [];
}

public class Matricula
{
    public Guid UsuarioId { get; set; }
    public Usuario? Usuario { get; set; }
    public Guid CursoId { get; set; }
    public Curso? Curso { get; set; }
    public RolUsuario RolEnCurso { get; set; }
}

public class Clase
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CursoId { get; set; }
    public Curso? Curso { get; set; }
    public string Titulo { get; set; } = "";
    public DateTimeOffset Inicio { get; set; }
    public int Orden { get; set; }

    /// <summary>
    /// RF-20: el docente decide si el tutor puede ampliar fuera del material. Por defecto
    /// no: el tutor se limita al contenido de la clase.
    /// </summary>
    public bool AmpliacionPermitida { get; set; }

    public List<ArchivoContenido> Archivos { get; set; } = [];
    public Examen? Examen { get; set; }
}
