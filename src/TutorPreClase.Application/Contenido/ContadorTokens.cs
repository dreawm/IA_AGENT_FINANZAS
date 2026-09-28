namespace TutorPreClase.Application.Contenido;

/// <summary>
/// Estimación de tokens sin depender del tokenizador de un proveedor: ~4 caracteres
/// por token para español. Sirve para el tope del contexto y el aviso al docente.
/// </summary>
public static class ContadorTokens
{
    private const double CaracteresPorToken = 4.0;

    public static int Estimar(string texto) =>
        string.IsNullOrEmpty(texto) ? 0 : (int)Math.Ceiling(texto.Length / CaracteresPorToken);
}
