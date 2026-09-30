# syntax=docker/dockerfile:1
#
# Imagen de la API para Railway. El CD la construye con `railway up` desde la raíz del
# repositorio, así que este archivo y railway.toml están en la raíz del contexto.
#
# Sin `--mount=type=cache`: Railway exige que el id del montaje empiece por
# `s/<id del servicio>-`, y un mismo Dockerfile no puede nombrar el servicio de dos
# entornos.

# ── Compilación ──────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS compilacion
WORKDIR /src

# Los .csproj van solos primero: la restauración se reutiliza si solo cambia el código.
COPY src/TutorPreClase.Domain/TutorPreClase.Domain.csproj src/TutorPreClase.Domain/
COPY src/TutorPreClase.Application/TutorPreClase.Application.csproj src/TutorPreClase.Application/
COPY src/TutorPreClase.Infrastructure/TutorPreClase.Infrastructure.csproj src/TutorPreClase.Infrastructure/
COPY src/TutorPreClase.Api/TutorPreClase.Api.csproj src/TutorPreClase.Api/
RUN dotnet restore src/TutorPreClase.Api/TutorPreClase.Api.csproj

COPY src/ src/
RUN dotnet publish src/TutorPreClase.Api/TutorPreClase.Api.csproj \
    --configuration Release --no-restore --output /app

# ── Ejecución ────────────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

# Npgsql carga GSSAPI al conectar; sin la biblioteca lo avisa en cada arranque.
RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=compilacion /app .

# El material del docente viaja con la imagen: en Railway la carpeta de contenido es la
# del repositorio (course-content/<correo del profesor>/<curso>/<clase>).
COPY course-content /contenido

# /datos es el volumen de Railway: archivos subidos y claves de Data Protection. Se
# ejecuta como root porque Railway monta los volúmenes con ese dueño.
ENV ASPNETCORE_ENVIRONMENT=Production \
    CarpetaContenido__Ruta=/contenido \
    Almacen__Raiz=/datos/almacen \
    ProteccionDatos__Carpeta=/datos/claves \
    Base__MigrarAlArrancar=true

# Railway inyecta PORT (8080 por defecto); en local vale lo mismo.
EXPOSE 8080
CMD ["sh", "-c", "ASPNETCORE_HTTP_PORTS=${PORT:-8080} exec dotnet TutorPreClase.Api.dll"]
