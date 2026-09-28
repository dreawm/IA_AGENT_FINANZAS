namespace TutorPreClase.Application.Llm;

/// <summary>
/// Resuelve el ILlmProvider del agente elegido. La credencial se pasa por llamada
/// porque es del alumno (BYOK), no de la plataforma.
/// </summary>
public interface IProveedorLlmFactory
{
    ILlmProvider Obtener(string agenteId, string apiKey);
    bool Existe(string agenteId);
}
