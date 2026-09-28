using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using TutorPreClase.Api.Endpoints;
using TutorPreClase.Api.Seguridad;
using TutorPreClase.Application.Evaluacion;
using TutorPreClase.Application.Llm;
using TutorPreClase.Application.Reportes;
using TutorPreClase.Infrastructure;
using TutorPreClase.Infrastructure.Contenido;
using TutorPreClase.Infrastructure.Persistencia;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AgregarInfraestructura(builder.Configuration);
builder.Services.AddScoped<IServicioReporte, ServicioReporte>();

builder.Services.AddSingleton<IColaExtraccion, ColaExtraccionEnMemoria>();
builder.Services.AddHostedService<ServicioExtraccionEnSegundoPlano>();

builder.Services.AddProblemDetails();
builder.Services.AddCors(o => o.AddDefaultPolicy(p => p
    .WithOrigins(builder.Configuration.GetSection("Cors:Origenes").Get<string[]>() ?? ["http://localhost:4200"])
    .AllowAnyHeader()
    .AllowAnyMethod()));

var autenticacion = builder.Services.AddAuthentication(opciones =>
{
    opciones.DefaultScheme = builder.Environment.IsProduction()
        ? JwtBearerDefaults.AuthenticationScheme
        : ManejadorAutenticacionDesarrollo.Esquema;
});

var autoridad = builder.Configuration["Oidc:Authority"];
if (!string.IsNullOrWhiteSpace(autoridad))
{
    autenticacion.AddJwtBearer(opciones =>
    {
        opciones.Authority = autoridad;
        opciones.Audience = builder.Configuration["Oidc:Audience"];
    });
}

// Sin OIDC configurado la API sigue levantando en desarrollo con cabeceras de prueba.
if (!builder.Environment.IsProduction())
{
    autenticacion.AddScheme<OpcionesAutenticacionDesarrollo, ManejadorAutenticacionDesarrollo>(
        ManejadorAutenticacionDesarrollo.Esquema, _ => { });
}

builder.Services.AddAuthorization();

var app = builder.Build();

// Fuera de produccion conviene ver el detalle del fallo, no un 500 opaco.
if (app.Environment.IsProduction()) app.UseExceptionHandler();
else app.UseDeveloperExceptionPage();

app.UseStatusCodePages();
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Los errores de negocio del examen viajan como 409 con su codigo, no como 500.
app.Use(async (contexto, siguiente) =>
{
    try
    {
        await siguiente(contexto);
    }
    catch (ExamenException ex)
    {
        contexto.Response.StatusCode = StatusCodes.Status409Conflict;
        await contexto.Response.WriteAsJsonAsync(new { error = ex.Codigo, mensaje = ex.Message });
    }
    catch (CredencialException ex)
    {
        contexto.Response.StatusCode = StatusCodes.Status400BadRequest;
        await contexto.Response.WriteAsJsonAsync(new { error = ex.Codigo, mensaje = ex.Message });
    }
    catch (UnauthorizedAccessException)
    {
        contexto.Response.StatusCode = StatusCodes.Status401Unauthorized;
    }
});

app.MapGet("/salud", () => Results.Ok(new { estado = "ok" })).AllowAnonymous();

app.MapearAlumno();
app.MapearCredenciales();
app.MapearDocente();
app.MapearAdmin();

if (app.Environment.IsDevelopment())
{
    using var ambito = app.Services.CreateScope();
    var db = ambito.ServiceProvider.GetRequiredService<AppDbContext>();

    // Las migraciones son de PostgreSQL; con Sqlite se crea el esquema directo.
    if (db.Database.IsNpgsql()) await db.Database.MigrateAsync();
    else await db.Database.EnsureCreatedAsync();

    await DatosIniciales.SembrarAsync(db);

    if (app.Configuration.GetValue("Demo:Sembrar", false))
    {
        var log = ambito.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Demo");
        var ids = await DatosDemo.SembrarAsync(db, log);
        app.MapGet("/demo", () => Results.Ok(ids)).AllowAnonymous();
    }
}

app.Run();

public partial class Program;
