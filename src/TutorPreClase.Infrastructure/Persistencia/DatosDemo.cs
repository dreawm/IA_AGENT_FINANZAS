using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TutorPreClase.Domain.Entidades;

namespace TutorPreClase.Infrastructure.Persistencia;

public sealed record IdentidadesDemo(Guid DocenteId, Guid AlumnoId, Guid CursoId, Guid ClaseId);

/// <summary>
/// Datos de demostracion para probar la plataforma de punta a punta. No inventan cursos
/// ni material: los cursos y clases salen de la carpeta de contenido (SDD §6.1). Aqui solo
/// se crean un docente, una alumna y un administrador, y los dos primeros se matriculan en
/// todos los cursos. Los examenes
/// tampoco: cada clase tiene el suyo y la IA genera las preguntas de cada alumno (RF-04).
/// Se activa con Demo:Sembrar = true y nunca deberia usarse en produccion.
/// </summary>
public static class DatosDemo
{
    private const string EmailDocente = "docente.demo@uni.edu";
    private const string EmailAlumna = "alumna.demo@uni.edu";
    private const string EmailAdmin = "admin.demo@uni.edu";

    public static async Task<IdentidadesDemo> SembrarAsync(AppDbContext db, ILogger log, CancellationToken ct = default)
    {
        var docente = await UsuarioAsync(db, EmailDocente, "Docente Demo", RolUsuario.Docente, ct);
        var alumna = await UsuarioAsync(db, EmailAlumna, "Alumna Demo", RolUsuario.Alumno, ct);
        await UsuarioAsync(db, EmailAdmin, "Administración Demo", RolUsuario.Admin, ct);

        // Tambien los cursos que aparezcan despues en la carpeta, al reiniciar.
        foreach (var cursoId in await db.Cursos.Select(c => c.Id).ToListAsync(ct))
        {
            await MatricularAsync(db, docente, cursoId, ct);
            await MatricularAsync(db, alumna, cursoId, ct);
        }

        await db.SaveChangesAsync(ct);

        var primera = await db.Clases
            .OrderBy(c => c.Curso!.Codigo).ThenBy(c => c.Orden)
            .FirstOrDefaultAsync(ct);

        if (primera is null)
        {
            log.LogWarning("Demo sin clases: la carpeta de contenido esta vacia o no configurada");
            return Anunciar(log, new IdentidadesDemo(docente.Id, alumna.Id, Guid.Empty, Guid.Empty));
        }

        return Anunciar(log, new IdentidadesDemo(docente.Id, alumna.Id, primera.CursoId, primera.Id));
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

    private static async Task MatricularAsync(AppDbContext db, Usuario usuario, Guid cursoId, CancellationToken ct)
    {
        if (await db.Matriculas.AnyAsync(m => m.UsuarioId == usuario.Id && m.CursoId == cursoId, ct)) return;
        db.Matriculas.Add(new Matricula { UsuarioId = usuario.Id, CursoId = cursoId, RolEnCurso = usuario.Rol });
    }

    private static IdentidadesDemo Anunciar(ILogger log, IdentidadesDemo ids)
    {
        log.LogInformation(
            "Demo lista — docente={Docente} alumna={Alumna} curso={Curso} clase={Clase}",
            ids.DocenteId, ids.AlumnoId, ids.CursoId, ids.ClaseId);

        return ids;
    }
}
