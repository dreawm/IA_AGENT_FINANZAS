namespace TutorPreClase.Application.Abstracciones;

public interface IRelojSistema
{
    DateTimeOffset Ahora { get; }
}

public sealed class RelojSistema : IRelojSistema
{
    public DateTimeOffset Ahora => DateTimeOffset.UtcNow;
}
