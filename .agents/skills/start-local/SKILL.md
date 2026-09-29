---
name: start-local
description: Levanta el Tutor Pre-Clase en local — API .NET en :5080 y el chat Angular en :4200, con base de datos y datos de demo listos. Úsalo cuando pidan arrancar, levantar, correr o abrir el proyecto, o cuando necesites la app en marcha para verificar un cambio.
---

# Levantar el Tutor Pre-Clase

Deja la plataforma corriendo y devuelve las URLs y los identificadores de prueba. Nada más:
no recorre funcionalidades ni toca datos salvo el sembrado de demo.

## 1. Comprobar si ya está arriba

```bash
curl -s --max-time 2 http://localhost:5080/salud && echo " <- API ya levantada"
curl -s --max-time 2 -o /dev/null http://localhost:4200 && echo "web ya levantada"
```

Si ya responden, no arranques otra instancia: usa esas y salta al paso 4.

## 2. API

Por defecto va sobre SQLite con datos de demo (`appsettings.Development.json`), así que no
necesita Docker:

```bash
cd <raíz del repo>
(ASPNETCORE_ENVIRONMENT=Development ASPNETCORE_URLS=http://localhost:5080 \
 dotnet run --project src/TutorPreClase.Api --no-launch-profile > api.log 2>&1 &)

for i in $(seq 1 30); do curl -s http://localhost:5080/salud && break; sleep 2; done
```

El primer arranque compila y crea el esquema: puede tardar ~20 s. Si tras el bucle no
responde, mira `api.log` en vez de reintentar a ciegas.

Con PostgreSQL en lugar de SQLite (lo que usa producción):

```bash
docker compose up -d
dotnet dotnet-ef database update --project src/TutorPreClase.Infrastructure --startup-project src/TutorPreClase.Api
```

y arranca sin la sección `Base:Proveedor` de `appsettings.Development.json`.

## 3. Frontend

```bash
(cd web && npm start > ../web.log 2>&1 &)
for i in $(seq 1 40); do curl -s -o /dev/null http://localhost:4200 && break; sleep 2; done
```

Si `node_modules` no existe, primero `cd web && npm ci`.

## 4. Datos para entrar

```bash
curl -s http://localhost:5080/demo
```

Devuelve `docenteId`, `alumnoId`, `cursoId` y `claseId` del curso sembrado. El `alumnoId` es
lo que se pega en la pantalla de entrada de la web.

Informa al usuario de:

- **Chat del alumno** → http://localhost:4200 (entrar con el `alumnoId`)
- **API** → http://localhost:5080 (`/salud`, `/demo`)
- Que el docente todavía no tiene pantalla: su parte va por API con las cabeceras
  `X-Usuario-Id: <docenteId>` y `X-Usuario-Rol: Docente`.
- Que para conversar hace falta conectar una credencial propia de IA en el panel
  "Conecta tu agente" (una clave de API de Anthropic o de OpenAI; las suscripciones de
  Codex.ai Pro o ChatGPT Plus no sirven). **Nunca pidas ni escribas esa clave tú.**

## 5. Parar

```bash
taskkill //F //IM TutorPreClase.Api.exe
taskkill //F //IM node.exe
rm -f api.log web.log
```

Para empezar de cero (borra también la credencial conectada y el contenido subido):

```bash
rm -f src/TutorPreClase.Api/tutorpreclase-dev.db
rm -rf src/TutorPreClase.Api/almacen
```
