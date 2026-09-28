using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TutorPreClase.Domain.Entidades;

namespace TutorPreClase.Infrastructure.Persistencia;

public sealed record IdentidadesDemo(Guid DocenteId, Guid AlumnoId, Guid CursoId, Guid ClaseId);

/// <summary>
/// Datos de demostracion para levantar la plataforma y probarla de punta a punta:
/// un curso, una clase con examen publicado, un docente y una alumna matriculada.
/// Se activa con Demo:Sembrar = true y nunca deberia usarse en produccion.
/// </summary>
public static class DatosDemo
{
    private const string EmailDocente = "docente.demo@uni.edu";
    private const string EmailAlumna = "alumna.demo@uni.edu";

    public static async Task<IdentidadesDemo> SembrarAsync(AppDbContext db, ILogger log, CancellationToken ct = default)
    {
        var docente = await db.Usuarios.FirstOrDefaultAsync(u => u.Email == EmailDocente, ct);

        if (docente is not null)
        {
            var alumnaExistente = await db.Usuarios.FirstAsync(u => u.Email == EmailAlumna, ct);
            var claseExistente = await db.Clases.FirstAsync(ct);

            return Anunciar(log, new IdentidadesDemo(
                docente.Id, alumnaExistente.Id, claseExistente.CursoId, claseExistente.Id));
        }

        docente = new Usuario { Email = EmailDocente, Nombre = "Docente Demo", Rol = RolUsuario.Docente };
        var alumna = new Usuario { Email = EmailAlumna, Nombre = "Alumna Demo", Rol = RolUsuario.Alumno };
        var curso = new Curso { Codigo = "IA101", Nombre = "Redes Neuronales", Periodo = "2026-2" };

        var clase = new Clase
        {
            CursoId = curso.Id,
            Titulo = "Clase 03 - Redes profundas",
            Inicio = DateTimeOffset.UtcNow.AddDays(1),
            Orden = 3,
            AmpliacionPermitida = true
        };

        var examen = new Examen
        {
            ClaseId = clase.Id,
            AbreEn = DateTimeOffset.UtcNow.AddHours(-1),
            CierraEn = DateTimeOffset.UtcNow.AddDays(1),
            MaxIntentos = 3,
            ModoFeedback = ModoFeedback.AlFinal,
            Publicado = true
        };

        examen.Preguntas.Add(new Pregunta
        {
            Enunciado = "Que funcion de activacion evita el desvanecimiento del gradiente en capas profundas?",
            Justificacion = "ReLU mantiene gradiente 1 para entradas positivas.",
            Tema = "Funciones de activacion",
            Orden = 1,
            Origen = OrigenPregunta.IA,
            Aprobada = true,
            Alternativas =
            [
                new Alternativa { Letra = "A", Texto = "ReLU", EsCorrecta = true },
                new Alternativa { Letra = "B", Texto = "Sigmoide", EsCorrecta = false },
                new Alternativa { Letra = "C", Texto = "Tanh", EsCorrecta = false },
                new Alternativa { Letra = "D", Texto = "Softmax", EsCorrecta = false }
            ]
        });

        examen.Preguntas.Add(new Pregunta
        {
            Enunciado = "Que problema presenta la sigmoide en redes profundas?",
            Justificacion = "Satura en los extremos y el gradiente se desvanece.",
            Tema = "Funciones de activacion",
            Orden = 2,
            Origen = OrigenPregunta.IA,
            Aprobada = true,
            Alternativas =
            [
                new Alternativa { Letra = "A", Texto = "Satura y desvanece el gradiente", EsCorrecta = true },
                new Alternativa { Letra = "B", Texto = "No es derivable", EsCorrecta = false },
                new Alternativa { Letra = "C", Texto = "Solo admite entradas positivas", EsCorrecta = false },
                new Alternativa { Letra = "D", Texto = "No converge nunca", EsCorrecta = false }
            ]
        });

        db.Usuarios.AddRange(docente, alumna);
        db.Cursos.Add(curso);
        db.Clases.Add(clase);
        db.Examenes.Add(examen);
        db.Matriculas.Add(new Matricula { UsuarioId = alumna.Id, CursoId = curso.Id, RolEnCurso = RolUsuario.Alumno });
        db.Matriculas.Add(new Matricula { UsuarioId = docente.Id, CursoId = curso.Id, RolEnCurso = RolUsuario.Docente });

        await db.SaveChangesAsync(ct);

        return Anunciar(log, new IdentidadesDemo(docente.Id, alumna.Id, curso.Id, clase.Id));
    }

    private static IdentidadesDemo Anunciar(ILogger log, IdentidadesDemo ids)
    {
        log.LogInformation(
            "Demo lista — docente={Docente} alumna={Alumna} curso={Curso} clase={Clase}",
            ids.DocenteId, ids.AlumnoId, ids.CursoId, ids.ClaseId);

        return ids;
    }
}
