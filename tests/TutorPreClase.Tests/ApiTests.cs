using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using TutorPreClase.Application.Llm;
using TutorPreClase.Tests.Infraestructura;

namespace TutorPreClase.Tests;

public class ApiTests
{
    [Fact]
    public async Task La_api_responde_salud_sin_autenticacion()
    {
        using var api = new ApiDePruebas();
        api.Sembrar();

        var respuesta = await api.CreateClient().GetAsync("/salud");

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
    }

    [Fact]
    public async Task Sin_identidad_el_chat_no_es_accesible()
    {
        using var api = new ApiDePruebas();
        var (_, _, _, claseId) = api.Sembrar();

        var respuesta = await api.CreateClient()
            .PostAsJsonAsync($"/api/v1/clases/{claseId}/conversacion", new { agenteId = "claude" });

        Assert.Equal(HttpStatusCode.Unauthorized, respuesta.StatusCode);
    }

    [Fact]
    public async Task Un_alumno_no_puede_usar_los_endpoints_del_docente()
    {
        using var api = new ApiDePruebas();
        var (_, alumnoId, _, claseId) = api.Sembrar();

        var respuesta = await api.Como(alumnoId, "Alumno").GetAsync($"/api/v1/clases/{claseId}/reporte");

        Assert.Equal(HttpStatusCode.Forbidden, respuesta.StatusCode);
    }

    [Fact]
    public async Task Un_alumno_no_puede_leer_la_conversacion_de_otro()
    {
        using var api = new ApiDePruebas();
        var (_, alumnoId, _, claseId) = api.Sembrar();

        var abierta = await api.Como(alumnoId, "Alumno")
            .PostAsJsonAsync($"/api/v1/clases/{claseId}/conversacion", new { agenteId = "claude" });

        var cuerpo = await abierta.Content.ReadFromJsonAsync<JsonElement>();
        var conversacionId = cuerpo.GetProperty("conversacionId").GetString();

        var intruso = await api.Como(Guid.NewGuid(), "Alumno")
            .GetAsync($"/api/v1/conversaciones/{conversacionId}/mensajes");

        Assert.Equal(HttpStatusCode.Forbidden, intruso.StatusCode);
    }

    [Fact]
    public async Task El_docente_arma_la_clase_y_el_alumno_conversa_por_sse()
    {
        using var api = new ApiDePruebas();
        var (docenteId, alumnoId, _, claseId) = api.Sembrar();

        var docente = api.Como(docenteId, "Docente");
        var alumno = api.Como(alumnoId, "Alumno");

        // 1. El docente configura y publica el examen.
        var examen = await docente.PutAsJsonAsync($"/api/v1/clases/{claseId}/examen", new
        {
            abreEn = DateTimeOffset.UtcNow.AddHours(-1),
            cierraEn = DateTimeOffset.UtcNow.AddHours(5),
            maxIntentos = 3,
            minutosLimite = (int?)null,
            modoFeedback = "AlFinal"
        });

        Assert.Equal(HttpStatusCode.OK, examen.StatusCode);
        var examenId = (await examen.Content.ReadFromJsonAsync<JsonElement>()).GetProperty("examenId").GetString();

        var pregunta = await docente.PostAsJsonAsync($"/api/v1/examenes/{examenId}/preguntas", new
        {
            enunciado = "Que funcion evita el desvanecimiento del gradiente?",
            justificacion = "ReLU mantiene gradiente 1 para entradas positivas.",
            tema = "Activaciones",
            nivel = (string?)null,
            orden = 1,
            aprobada = true,
            alternativas = new[]
            {
                new { letra = "A", texto = "ReLU", esCorrecta = true },
                new { letra = "B", texto = "Sigmoide", esCorrecta = false }
            }
        });

        Assert.Equal(HttpStatusCode.Created, pregunta.StatusCode);
        Assert.Equal(HttpStatusCode.OK,
            (await docente.PostAsync($"/api/v1/examenes/{examenId}/publicar", null)).StatusCode);

        // 2. El alumno abre su chat.
        var abierta = await alumno.PostAsJsonAsync($"/api/v1/clases/{claseId}/conversacion", new { agenteId = "claude" });
        var conversacionId = (await abierta.Content.ReadFromJsonAsync<JsonElement>())
            .GetProperty("conversacionId").GetString();

        // 3. Y conversa: la respuesta llega como stream SSE.
        api.Proveedor.Responde(
            new TextoParcial("Hola, revisemos la clase."),
            new Fin(12, 8));

        var mensaje = await alumno.PostAsJsonAsync(
            $"/api/v1/conversaciones/{conversacionId}/mensajes", new { texto = "hola" });

        Assert.Equal("text/event-stream", mensaje.Content.Headers.ContentType?.MediaType);

        var flujo = await mensaje.Content.ReadAsStringAsync();

        Assert.Contains("event: modo", flujo);
        Assert.Contains("event: token", flujo);
        Assert.Contains("Hola, revisemos la clase.", flujo);
        Assert.Contains("event: fin", flujo);

        // 4. El historial queda disponible para recargar la pagina.
        var historial = await alumno.GetFromJsonAsync<JsonElement>(
            $"/api/v1/conversaciones/{conversacionId}/mensajes");

        Assert.Equal(2, historial.GetArrayLength());
    }

    [Fact]
    public async Task El_reporte_del_docente_trae_niveles_y_dudas()
    {
        using var api = new ApiDePruebas();
        var (docenteId, _, _, claseId) = api.Sembrar();

        var reporte = await api.Como(docenteId, "Docente")
            .GetFromJsonAsync<JsonElement>($"/api/v1/clases/{claseId}/reporte");

        Assert.Equal("Clase 03 - Redes profundas", reporte.GetProperty("clase").GetString());
        Assert.True(reporte.TryGetProperty("distribucionNiveles", out _));
        Assert.True(reporte.TryGetProperty("dudasFueraDelMaterial", out _));
        Assert.True(reporte.TryGetProperty("preguntasMasFalladas", out _));
    }

    [Fact]
    public async Task El_docente_apaga_la_ampliacion_de_una_clase()
    {
        using var api = new ApiDePruebas();
        var (docenteId, _, _, claseId) = api.Sembrar();

        var respuesta = await api.Como(docenteId, "Docente")
            .PutAsJsonAsync($"/api/v1/clases/{claseId}/ampliacion", new { permitida = false });

        Assert.Equal(HttpStatusCode.OK, respuesta.StatusCode);
        var cuerpo = await respuesta.Content.ReadFromJsonAsync<JsonElement>();
        Assert.False(cuerpo.GetProperty("ampliacionPermitida").GetBoolean());

        using var db = api.NuevoContexto();
        Assert.False(db.Clases.Single().AmpliacionPermitida);
    }
}
