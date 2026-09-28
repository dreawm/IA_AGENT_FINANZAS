using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TutorPreClase.Application.Abstracciones;
using TutorPreClase.Application.Contenido;
using TutorPreClase.Application.Evaluacion;
using TutorPreClase.Application.Llm;
using TutorPreClase.Application.Nivel;
using TutorPreClase.Application.Tutor;
using TutorPreClase.Infrastructure.Contenido;
using TutorPreClase.Infrastructure.Llm;
using TutorPreClase.Infrastructure.Persistencia;

namespace TutorPreClase.Infrastructure;

public static class RegistroServicios
{
    public static IServiceCollection AgregarInfraestructura(
        this IServiceCollection servicios, IConfiguration config)
    {
        // Postgres es el proveedor del SDD; Sqlite permite levantar en local o en
        // pruebas sin Docker. Solo se registra uno: EF no admite dos a la vez.
        var proveedor = config["Base:Proveedor"] ?? "Postgres";

        servicios.AddDbContext<AppDbContext>(o =>
        {
            if (proveedor.Equals("Sqlite", StringComparison.OrdinalIgnoreCase))
                o.UseSqlite(config.GetConnectionString("Sqlite") ?? "DataSource=tutorpreclase.db");
            else
                o.UseNpgsql(config.GetConnectionString("Postgres"));
        });

        servicios.AddScoped<IAppDbContext>(sp => sp.GetRequiredService<AppDbContext>());

        servicios.AddMemoryCache();
        servicios.AddSingleton<IRelojSistema, RelojSistema>();

        servicios.Configure<OpcionesContextoClase>(config.GetSection(OpcionesContextoClase.Seccion));
        servicios.Configure<OpcionesAgentes>(config.GetSection(OpcionesAgentes.Seccion));

        servicios.AddScoped<IContextoClaseService, ContextoClaseService>();
        servicios.AddScoped<INivelService, NivelService>();
        servicios.AddScoped<IServicioExamen, ServicioExamen>();
        servicios.AddScoped<IEjecutorHerramientas, EjecutorHerramientas>();
        servicios.AddScoped<IAgenteTutorService, AgenteTutorService>();

        servicios.AddSingleton<IAlmacenArchivos>(_ =>
            new AlmacenArchivosLocal(config["Almacen:Raiz"] ?? "almacen"));
        servicios.AddScoped<IServicioContenido, ServicioContenido>();

        servicios.AddSingleton<IExtractorTexto, ExtractorPdf>();
        servicios.AddSingleton<IExtractorTexto, ExtractorPptx>();
        servicios.AddSingleton<IExtractorTexto, ExtractorDocx>();
        servicios.AddSingleton<IExtractorTexto, ExtractorTextoPlano>();
        servicios.AddSingleton<IExtractorTextoFactory, ExtractorTextoFactory>();

        servicios.AddDataProtection();
        servicios.AddSingleton<IValidadorCredencial, ValidadorCredencial>();
        servicios.AddScoped<IBovedaCredenciales, BovedaCredenciales>();
        servicios.AddSingleton<IProveedorLlmFactory, ProveedorLlmFactory>();
        AgregarClientesLlm(servicios, config);

        return servicios;
    }

    private static void AgregarClientesLlm(IServiceCollection servicios, IConfiguration config)
    {
        var agentes = config.GetSection(OpcionesAgentes.Seccion).Get<OpcionesAgentes>() ?? [];

        foreach (var (id, opciones) in agentes)
        {
            servicios.AddHttpClient($"llm:{id}", cliente =>
            {
                if (!string.IsNullOrWhiteSpace(opciones.BaseUrl))
                    cliente.BaseAddress = new Uri(opciones.BaseUrl);

                cliente.Timeout = TimeSpan.FromMinutes(2);
            });
        }
    }
}
