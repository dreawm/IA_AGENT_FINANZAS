namespace TutorPreClase.Application.Llm;

/// <summary>Sección "Agentes" de configuración (SDD §5.8).</summary>
public sealed class OpcionesAgentes : Dictionary<string, OpcionesAgente>
{
    public const string Seccion = "Agentes";
}

public sealed class OpcionesAgente
{
    public string Nombre { get; set; } = "";
    public string BaseUrl { get; set; } = "";
    public string Modelo { get; set; } = "";
    public string? Descripcion { get; set; }
    public bool Habilitado { get; set; } = true;
    public string? ApiKey { get; set; }
}
