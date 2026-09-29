namespace TutorPreClase.Application.Llm;

/// <summary>
/// Conexion de un agente por inicio de sesion en el proveedor, con OAuth y PKCE (RF-29,
/// SDD §5.1). La clave la obtiene y la guarda la API: el navegador solo transporta un
/// codigo de un solo uso.
/// </summary>
public interface IConexionOAuth
{
    /// <summary>Genera el verificador del alumno y devuelve la URL de autorizacion.</summary>
    Task<string> IniciarAsync(Guid usuarioId, string agenteId, CancellationToken ct = default);

    /// <summary>Canjea el codigo con el verificador de ese alumno y guarda la clave.</summary>
    Task<CredencialResumen> CanjearAsync(Guid usuarioId, string agenteId, string codigo, CancellationToken ct = default);
}
