using Microsoft.EntityFrameworkCore;
using TutorPreClase.Application.Abstracciones;
using TutorPreClase.Domain.Entidades;

namespace TutorPreClase.Application.Academico;

/// <summary>
/// Las matrículas se derivan del profesor (RF-33): el profesor está en sus cursos y el
/// alumno, en los cursos del profesor que eligió, y en ningún otro. No guarda cambios: lo
/// hace quien llama.
/// </summary>
public static class MatriculasPorProfesor
{
    /// <summary>
    /// Deja al alumno exactamente en los cursos de su profesor. Un profesor que entra como
    /// alumno conserva además los suyos.
    /// </summary>
    public static async Task AlinearAlumnoAsync(IAppDbContext db, Usuario alumno, CancellationToken ct)
    {
        var profesorId = alumno.ProfesorId;
        var deseados = await db.Cursos
            .Where(c => (profesorId != null && c.DocenteId == profesorId) || c.DocenteId == alumno.Id)
            .Select(c => c.Id)
            .ToListAsync(ct);

        var actuales = await db.Matriculas.Where(m => m.UsuarioId == alumno.Id).ToListAsync(ct);

        db.Matriculas.RemoveRange(actuales.Where(m => !deseados.Contains(m.CursoId)));

        foreach (var cursoId in deseados.Where(id => actuales.All(m => m.CursoId != id)))
            db.Matriculas.Add(new Matricula { Usuario = alumno, CursoId = cursoId, RolEnCurso = RolUsuario.Alumno });
    }

    /// <summary>Un curso nuevo del profesor llega a él y a todos los alumnos que lo eligieron.</summary>
    public static async Task AlinearCursoAsync(IAppDbContext db, Curso curso, CancellationToken ct)
    {
        if (curso.DocenteId is not Guid profesorId) return;

        var matriculados = await db.Matriculas.Where(m => m.CursoId == curso.Id).Select(m => m.UsuarioId).ToListAsync(ct);

        var faltan = await db.Usuarios
            .Where(u => (u.Id == profesorId || u.ProfesorId == profesorId) && !matriculados.Contains(u.Id))
            .Select(u => new { u.Id, u.Rol })
            .ToListAsync(ct);

        foreach (var u in faltan)
            db.Matriculas.Add(new Matricula { UsuarioId = u.Id, CursoId = curso.Id, RolEnCurso = u.Rol });
    }
}
