namespace TutorPreClase.Domain.Entidades;

public enum RolUsuario { Alumno, Docente, Admin }

/// <summary>Estado de extracción de texto de un archivo de contenido (SDD §4).</summary>
public enum EstadoArchivo { Pendiente, Procesando, Listo, Error }

public enum ModoFeedback { Inmediato, AlFinal }

public enum EstadoIntento { EnCurso, Enviado, EnRevision }

/// <summary>Modo del chat del alumno (SDD §5).</summary>
public enum ModoConversacion { Consulta, Evaluacion, Revision }

public enum RolMensaje { Alumno, Agente, Herramienta }

public enum NivelAlumnoValor { Inicial, Basico, Intermedio, Avanzado }

public enum OrigenNivel { Automatico, Tutor, Docente }

public enum OrigenPregunta { IA, Docente }
