using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TutorPreClase.Application.Academico;
using TutorPreClase.Domain.Entidades;

namespace TutorPreClase.Infrastructure.Persistencia;

public sealed record IdentidadesDemo(Guid DocenteId, Guid AlumnoId, Guid CursoId, Guid ClaseId);

/// <summary>
/// Datos de demostracion para probar la plataforma de punta a punta. No inventan cursos,
/// material ni profesores: salen de la carpeta de contenido, una carpeta por profesor
/// (SDD §6.1). Aqui solo se crean una alumna, que elige como profesor al dueño del primer
/// curso, y un administrador. Los examenes tampoco: la IA genera las preguntas de cada
/// alumno (RF-04). Se activa con Demo:Sembrar = true y nunca deberia usarse en produccion.
/// </summary>
public static class DatosDemo
{
    private const string EmailAlumna = "alumna.demo@uni.edu";
    private const string EmailAdmin = "admin.demo@uni.edu";

    public static async Task<IdentidadesDemo> SembrarAsync(AppDbContext db, ILogger log, CancellationToken ct = default)
    {
        var primera = await db.Clases
            .Where(c => c.Curso!.DocenteId != null)
            .OrderBy(c => c.Curso!.Codigo).ThenBy(c => c.Orden)
            .Select(c => new { c.Id, c.CursoId, c.Curso!.DocenteId })
            .FirstOrDefaultAsync(ct);

        var alumna = await UsuarioAsync(db, EmailAlumna, "Alumna Demo", RolUsuario.Alumno, ct);
        await UsuarioAsync(db, EmailAdmin, "Administración Demo", RolUsuario.Admin, ct);

        alumna.ProfesorId ??= primera?.DocenteId;
        await MatriculasPorProfesor.AlinearAlumnoAsync(db, alumna, ct);
        await db.SaveChangesAsync(ct);

        if (primera is null)
        {
            log.LogWarning("Demo sin clases: la carpeta de contenido esta vacia o no tiene carpetas de profesor");
            return Anunciar(log, new IdentidadesDemo(Guid.Empty, alumna.Id, Guid.Empty, Guid.Empty));
        }

        return Anunciar(log, new IdentidadesDemo(primera.DocenteId!.Value, alumna.Id, primera.CursoId, primera.Id));
    }

    private static async Task<Usuario> UsuarioAsync(
        AppDbContext db, string email, string nombre, RolUsuario rol, CancellationToken ct)
    {
        var usuario = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == email, ct);
        if (usuario is not null) return usuario;

        usuario = new Usuario { Email = email, Nombre = nombre, Rol = rol };
        db.Usuarios.Add(usuario);
        return usuario;
    }

    private static IdentidadesDemo Anunciar(ILogger log, IdentidadesDemo ids)
    {
        log.LogInformation(
            "Demo lista — profesor={Docente} alumna={Alumna} curso={Curso} clase={Clase}",
            ids.DocenteId, ids.AlumnoId, ids.CursoId, ids.ClaseId);

        return ids;
    }
}
