---
name: functional-test
description: Prueba funcional del Tutor Pre-Clase trazada al SDD — recorre los requisitos RF-01 a RF-28 y los RNF verificables a mano, en la web y por API, y reporta cuáles pasan, cuáles fallan y cuáles no están implementados. Úsalo cuando pidan probar la app entera, validarla antes de una demo, o comprobar que un cambio no rompió el flujo completo.
---

# Prueba funcional según el SDD

Cada bloque verifica requisitos concretos de
[`specs/SDD-Plataforma-Evaluacion-PreClase.md`](../../../specs/SDD-Plataforma-Evaluacion-PreClase.md).
Si el spec cambió, **léelo antes**: manda el spec, no esta lista.

El arranque no está aquí: **invoca primero el skill `start-local`**.

## 0. Preparación

```bash
dotnet test
```

Gratis y en segundos: cubre RF-08, RF-09, RF-12, RF-15, RF-21, RF-24 a RF-28 y RNF-10 a
nivel de unidad e integración. Si algo falla ahí, arréglalo antes de abrir el navegador.

Toma los identificadores: `curl -s http://localhost:5080/demo` → `$DOC`, `$ALU`, `$CLA`.

**La credencial (BYOK).** El chat necesita una clave de API real de Anthropic o de OpenAI;
las suscripciones de claude.ai Pro y ChatGPT Plus no habilitan la API. La conecta la
persona en el bloque B desde la web. **Nunca la pidas por el chat, ni la escribas en un
archivo, un log o el resumen.** Sin clave, salta los bloques C, D y E y dilo al reportar.

## Bloque A — El docente prepara la clase

| Requisito | Cómo verificarlo |
| --- | --- |
| RF-01 | `POST /api/v1/cursos/$CUR/clases` crea una clase; `PUT /api/v1/clases/$CLA/examen` fija ventana e intentos |
| RF-02 | Sube un `.md` y un `.pdf` al mismo `POST /api/v1/clases/$CLA/contenido`; ambos se aceptan |
| RF-03 | `GET /api/v1/clases/$CLA/contenido` pasa a `"estado":"Listo"` con `paginas` ≥ 1 y `contextoClase.tokens` > 0 |
| RF-05 | `POST /api/v1/examenes/$EX/preguntas` exige exactamente una alternativa correcta y guarda la justificación |
| RF-14 | `PUT .../examen` con `"modoFeedback":"Inmediato"` y con `"AlFinal"`: ambos aceptados |
| RF-18 | `PUT .../examen` con `"minutosLimite":10`; luego el chat emite `segundosRestantes` en `progreso` |

```bash
printf 'La funcion sigmoide satura en los extremos y por eso reduce el gradiente.\nReLU mantiene gradiente 1 para entradas positivas y evita el desvanecimiento.\n' > <scratchpad>/Clase03.md

curl -s -X POST -H "X-Usuario-Id: $DOC" -H "X-Usuario-Rol: Docente" \
  -F "archivo=@<scratchpad>/Clase03.md" "http://localhost:5080/api/v1/clases/$CLA/contenido"

curl -s -H "X-Usuario-Id: $DOC" -H "X-Usuario-Rol: Docente" \
  "http://localhost:5080/api/v1/clases/$CLA/contenido"
```

## Bloque B — Credencial propia (BYOK)

Los bloques B a G se prueban **en la pantalla, con Chrome**; el `curl` solo complementa.
Invoca el skill `claude-in-chrome` y carga las herramientas en **una sola** llamada a ToolSearch:

```
select:mcp__claude-in-chrome__list_connected_browsers,mcp__claude-in-chrome__tabs_context_mcp,mcp__claude-in-chrome__tabs_create_mcp,mcp__claude-in-chrome__navigate,mcp__claude-in-chrome__computer,mcp__claude-in-chrome__read_page,mcp__claude-in-chrome__find,mcp__claude-in-chrome__form_input,mcp__claude-in-chrome__resize_window,mcp__claude-in-chrome__tabs_close_mcp
```

**Comprueba la conexión antes de seguir:** `list_connected_browsers` debe devolver al menos
un navegador. Si viene vacío o `tabs_context_mcp` responde *"Browser extension is not
connected"*, **detente y avisa**: la persona debe abrir Chrome con la extensión Claude
(https://claude.ai/chrome) iniciada con la misma cuenta de Claude Code. No sustituyas la
prueba de pantalla por `curl` en silencio; si decide seguir sin navegador, marca como
**no verificados** los requisitos de UI.

Con la conexión lista: `tabs_create_mcp`, navega a `http://localhost:4200` y entra con el
`alumnoId`.

| Requisito | Cómo verificarlo |
| --- | --- |
| RF-27 | Al entrar sin credencial, se abre el panel **"Conecta tu agente"** y el campo de mensaje está deshabilitado |
| RF-25 | Con una clave inventada, el proveedor la rechaza, sale el aviso y **no** se guarda (`GET /alumno/credenciales` sigue vacío) |
| RF-24 | La persona pega su clave real → el panel se cierra y el agente queda conectado; **Desconectar** lo revierte |
| RF-26 / RNF-10 | `GET /api/v1/alumno/credenciales` devuelve solo `agenteId`, `ultimos4`, `estado` y fechas. **Si aparece la clave completa, párate y repórtalo: es un fallo grave** |
| RF-26 | Desde otro `X-Usuario-Id` el mismo endpoint devuelve `[]` |
| RF-06 | El selector de la cabecera solo lista los agentes conectados |

```bash
curl -s -X PUT -H "X-Usuario-Id: $ALU" -H "X-Usuario-Rol: Alumno" \
  -H "Content-Type: application/json" -d '{"clave":"sk-ant-api03-esta-no-existe-0000"}' \
  "http://localhost:5080/api/v1/alumno/credenciales/claude"     # clave_rechazada

curl -s -H "X-Usuario-Id: $ALU" -H "X-Usuario-Rol: Alumno" \
  "http://localhost:5080/api/v1/alumno/credenciales"

curl -s -H "X-Usuario-Id: $(python -c 'import uuid;print(uuid.uuid4())')" \
  -H "X-Usuario-Rol: Alumno" "http://localhost:5080/api/v1/alumno/credenciales"   # []
```

## Bloque C — Modo Consulta

| Requisito | Cómo verificarlo |
| --- | --- |
| RF-19 | `¿qué dice el material sobre la sigmoide?` obtiene respuesta del tema |
| RF-11 | Esa respuesta trae **chip de fuente** `Clase03.md, p. 1` |
| RF-12 | `¿y qué es GELU, que no está en el material?` → lo dice, y amplía en bloque **"Fuera del material"**, ámbar y sin chip de fuente |
| RF-19 | `dame una receta de ceviche` → declina y reconduce al curso |
| RNF-02 | El primer token aparece en menos de ~3 s |

## Bloque D — Examen por chat

| Requisito | Cómo verificarlo |
| --- | --- |
| RF-06 | **Comenzar examen**: la cabecera pasa a *Examen en curso* y el selector de agente queda deshabilitado |
| RF-15 | Pide una pista o la respuesta: se niega y no filtra la correcta |
| RF-07 | Responde en lenguaje natural (`creo que es la A`): el progreso avanza y presenta la siguiente |
| RF-18 | Con `minutosLimite`, la cabecera muestra el tiempo restante |
| RF-09 | Al responder la última, comunica la **nota sobre 20**; `GET /api/v1/intentos/$INT/resultado` da la misma nota |
| RNF-01 | El progreso se actualiza al instante, sin esperar al modelo |
| RF-08 | Tras agotar los 3 intentos, iniciar otro responde `SinIntentosDisponibles` |

## Bloque E — Revisión

| Requisito | Cómo verificarlo |
| --- | --- |
| RF-10 | `revisemos mis errores` → explica por qué falló, el concepto correcto y una pregunta de comprobación |
| RF-11 | Esa explicación cita el material con **chip de fuente válido** |
| RF-12 | Una repregunta fuera del material vuelve a salir marcada como ampliación |
| RF-13 | `GET /api/v1/clases/$CLA/reporte` → `dudasFueraDelMaterial` incluye la consulta sobre GELU con `conAmpliacion: true` |

## Bloque F — Nivel, reporte y control del docente

| Requisito | Cómo verificarlo |
| --- | --- |
| RF-21 | El reporte trae el nivel del alumno acorde a su nota (0–10,9 Inicial · 11–14,9 Básico · 15–17,9 Intermedio · 18–20 Avanzado) |
| RF-22 | El reporte trae `distribucionNiveles`, nivel por alumno y `temasDebilesDelGrupo` |
| RF-16 | El reporte trae `promedioNota`, `distribucionNotas`, `preguntasMasFalladas` y `usoPorAgente` |
| RF-21 | `PUT /clases/$CLA/niveles/$ALU` con `{"nivel":"Avanzado"}` responde `"origen":"Docente"` y el cálculo no lo pisa |
| RF-20 | `PUT /clases/$CLA/ampliacion` con `{"permitida":false}`; recarga el chat y pregunta algo fuera del material: ya **no** sale el bloque de ampliación, sino *"Esto no está en el material de la clase; pregúntalo en la sesión."* — lo corta el servidor, no el prompt |

```bash
curl -s -H "X-Usuario-Id: $DOC" -H "X-Usuario-Rol: Docente" \
  "http://localhost:5080/api/v1/clases/$CLA/reporte" | python -m json.tool
```

## Bloque G — Seguridad y resiliencia

| Requisito | Cómo verificarlo |
| --- | --- |
| SDD §9.1 | Alumno contra `GET /clases/$CLA/reporte` → **403**; sin cabeceras → **401** |
| SDD §9.1 | Otro alumno contra `GET /conversaciones/$CONV/mensajes` → **403** |
| RF-28 | Desconecta la credencial y escribe en el chat: avisa `sin_credencial` y no llama al proveedor. Con una credencial que el proveedor rechace, se marca *Invalida* y pide reconectarla |
| RNF-06 | El prompt no lleva nombre ni correo del alumno (revisa `api.log`) |
| RNF-07 | Estrecha el navegador a ~400 px: la lista de clases pasa arriba y el chat sigue usable |

## Requisitos que hoy no se pueden dar por buenos

Dilo explícitamente al reportar; no los marques como aprobados:

| Requisito | Estado |
| --- | --- |
| RF-04 | La generación de preguntas con IA no está implementada: el docente las crea y aprueba por API |
| RF-05 | La justificación y la respuesta correcta sí se guardan; las **referencias al contenido** (`pregunta_referencia`) no tienen endpoint todavía |
| RF-17 | Cambiar de agente funciona en Consulta y Revisión, pero **durante el examen está bloqueado**: no se cumple "continuar con otro agente sin perder el avance" ante un fallo del proveedor |
| RF-23 | El banco adaptativo por nivel no filtra por nivel todavía |
| RNF-08 | No hay tope de mensajes por alumno y clase |
| RNF-03 / RNF-04 / RNF-05 | Carga, volumen y disponibilidad: no se verifican a mano, hacen falta k6 y medición en despliegue |

## Cerrar

Para parar los procesos, usa el paso 5 del skill `start-local`. Y si quieres dejarlo todo
limpio para la próxima — esto borra también la credencial conectada y el contenido subido:

```bash
rm -f src/TutorPreClase.Api/tutorpreclase-dev.db
rm -rf src/TutorPreClase.Api/almacen
```

## Al terminar

Reporta por bloques: qué requisito pasó, cuál falló y con qué evidencia, y cuáles se
saltaron (por falta de credencial o porque no están implementados). No des por bueno lo que
no comprobaste.

Un modelo real no repite frases exactas: verifica el **comportamiento** (que cite, que marque
la ampliación, que registre la respuesta, que no filtre la correcta), nunca el texto literal.
El recorrido completo son ~12 mensajes cortos al proveedor — céntimos, pero lo paga la
cuenta conectada: avisa si se va a repetir muchas veces.
