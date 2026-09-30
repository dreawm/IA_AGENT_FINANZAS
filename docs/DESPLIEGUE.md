# Despliegue en Railway y Vercel

La API (.NET) corre como contenedor en **Railway**, junto a su PostgreSQL. La web
(Angular) se publica como sitio estático en **Vercel**. **Quien despliega es GitHub
Actions**, no las plataformas: el CD (`.github/workflows/cd.yml`) solo arranca cuando el
CI termina en verde sobre ese commit exacto, y despliega primero la API y después la web.

| | Producción | Desarrollo (opcional) |
| --- | --- | --- |
| Rama | `main` | `develop` |
| Entorno de GitHub | `production` | `development` |
| Environment de Railway | `production` | `development` |
| Vercel | dominio de producción | *Preview* + alias `vars.VERCEL_ALIAS` |

> **Ningún dominio está en el repositorio.** Las URLs llegan por variables: `API_URL` en
> Vercel; `Cors__Origenes__0`, `Acceso__UrlRetorno` y `Agentes__openrouter__UrlRetorno` en
> Railway.

## Qué hay en el repositorio

| Archivo | Para qué |
| --- | --- |
| `Dockerfile` | Imagen de la API. Copia `course-content/` a `/contenido`, migra la base al arrancar y escucha en `$PORT` (8080) |
| `railway.toml` | Builder Dockerfile, healthcheck en `/salud`, reinicio limitado |
| `web/vercel.json` | Build `npm run build:deploy`, salida `dist/web/browser`, rutas SPA, auto-deploy de Vercel apagado en `main` y `develop` |
| `web/scripts/set-env.mjs` | Escribe `environment.prod.ts` con `API_URL`; falla si falta |
| `.github/workflows/ci.yml` | Pruebas .NET, build de la web como en Vercel, build de la imagen |
| `.github/workflows/cd.yml` | `railway up` → espera el estado final → `vercel deploy --prebuilt` |

## 1. Railway

1. **Proyecto nuevo** (por ejemplo `tutor-preclase`), environment `production`.
2. **PostgreSQL**: *New* → *Database* → *PostgreSQL*.
3. **Servicio de la API**: *New* → *Empty Service*, nombre **`api`**. **No lo conectes a
   GitHub**: lo despliega el CD con `railway up` y habría dos desplegadores compitiendo.
4. **Volumen** del servicio `api` montado en **`/datos`**. Ahí van los archivos subidos y
   las claves de Data Protection, que cifran las credenciales de los alumnos: sin volumen,
   cada despliegue las pierde y los alumnos tienen que volver a conectar OpenRouter.
5. **Variables** del servicio `api`:

   | Variable | Valor |
   | --- | --- |
   | `ConnectionStrings__Postgres` | `Host=${{Postgres.PGHOST}};Port=${{Postgres.PGPORT}};Database=${{Postgres.PGDATABASE}};Username=${{Postgres.PGUSER}};Password=${{Postgres.PGPASSWORD}}` |
   | `Acceso__ClaveSesion` | 48 bytes aleatorios en Base64 (abajo) |
   | `Acceso__UrlRetorno` | `https://<web>/entrar` |
   | `Acceso__Administradores__0` | el correo del administrador |
   | `Acceso__Proveedores__google__ClientId` | del JSON de Google (lo pega la persona) |
   | `Acceso__Proveedores__google__ClientSecret` | del JSON de Google (lo pega la persona) |
   | `Cors__Origenes__0` | `https://<web>` |
   | `Agentes__openrouter__UrlRetorno` | `https://<web>/conectar/openrouter` |

   > **La cadena de conexión va en formato clave=valor**, no como la `DATABASE_URL`
   > (`postgresql://…`) que genera Railway: Npgsql no acepta esa forma. Con las
   > referencias `${{Postgres.…}}` nadie ve la contraseña y la misma cadena sirve en
   > cualquier environment.
   >
   > **`Acceso__ClaveSesion` tiene que ser Base64.** Con otro texto la API arranca pero
   > responde 500 a todo. Genérala en tu equipo y pégala directamente en Railway:
   >
   > ```bash
   > python -c "import os,base64;print(base64.b64encode(os.urandom(48)).decode())"
   > ```

6. *Settings* → *Networking* → **Generate Domain**, puerto **8080**.
7. *Project Settings* → *Tokens* → token **de proyecto** del environment `production`.
   Es el `RAILWAY_TOKEN` de GitHub. El token de cuenta (*Account Settings*) no sirve.

## 2. Vercel

1. *Add New* → *Project* → importa `dreawm/IA_AGENT_FINANZAS`, **Root Directory `web`**.
   El resto sale de `web/vercel.json`.
2. Variable **`API_URL`** = `https://<dominio de Railway>/api`, en *Production*.
   **Sin marcarla como Sensitive**: una variable sensible no la puede leer `vercel pull`
   y el build falla con «Falta API_URL».
3. *Account Settings* → *Tokens* → token para el CD (`VERCEL_TOKEN`).
4. `VERCEL_ORG_ID` y `VERCEL_PROJECT_ID`: *Project Settings* → *General* (Project ID) y
   *Team/Account Settings* (Team/User ID).

## 3. GitHub

*Settings* → *Environments* → **`production`** → *Environment secrets*:

| Secreto | De dónde |
| --- | --- |
| `RAILWAY_TOKEN` | Railway, token de proyecto de `production` |
| `VERCEL_TOKEN` | Vercel, token de cuenta |
| `VERCEL_ORG_ID` | Vercel |
| `VERCEL_PROJECT_ID` | Vercel |

*Settings* → *Secrets and variables* → *Actions* → *Variables*: **`RAILWAY_SERVICE`** = `api`.

## 4. Google (y Microsoft)

En el cliente OAuth de Google (skill `setup-oauth`) añade:

- *Authorized JavaScript origins*: `https://<web>`
- *Authorized redirect URIs*: `https://<web>/entrar`

OpenRouter no necesita registro: la URL de retorno la manda la API.

## 5. Primer despliegue

Un push a `main` (o *Re-run* del último CI) lanza el CD. En Actions:
**CI** en verde → **CD** `production · main · <mensaje>` → *API → Railway* → *Web → Vercel*.
Si falta un secreto, el primer paso lo dice por su nombre.

Comprobación rápida:

```bash
curl https://<api>/salud                       # {"estado":"ok"}
curl https://<api>/api/v1/acceso/proveedores    # google configurado: true
```

## Notas

- **La base se migra sola al arrancar** (`Base__MigrarAlArrancar=true` en la imagen). Las
  migraciones son de PostgreSQL; se regeneran con `ASPNETCORE_ENVIRONMENT=Production` para
  que no salgan con tipos de SQLite.
- **PostgreSQL solo guarda fechas en UTC**: `AppDbContext` convierte las horas de clase
  (Lima, -05:00) al guardar, sin cambiar el instante.
- **El contenido del docente viaja en la imagen** (`course-content/`): subir material es
  hacer commit en esa carpeta y dejar que el CD despliegue.
- **Sin `--mount=type=cache` en el Dockerfile**: Railway exige un id de montaje por
  servicio y rompe el build.
- **Entorno de desarrollo**: rama `develop`, environment `development` en Railway y en
  GitHub (con su propio `RAILWAY_TOKEN` y su propia `Acceso__ClaveSesion`) y, si quieres
  un dominio fijo en Vercel, la variable `VERCEL_ALIAS`.
