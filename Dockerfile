# Usar la imagen base de .NET 9 SDK para build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copiar archivos de proyecto y restaurar dependencias
COPY *.csproj ./
RUN dotnet restore

# Copiar código fuente
COPY . ./

# Publicar la aplicación
RUN dotnet publish -c Release -o out

# Usar la imagen runtime de .NET 9 para producción
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Crear usuario no-root para seguridad
RUN addgroup --system --gid 1001 dotnet \
    && adduser --system --uid 1001 --ingroup dotnet dotnet

# Instalar SQLite
RUN apt-get update && apt-get install -y sqlite3 && rm -rf /var/lib/apt/lists/*

# Crear y configurar directorio de datos
RUN mkdir -p /app/data && chown -R dotnet:dotnet /app

# Copiar archivos publicados y script de inicio
COPY --from=build /app/out .
COPY start.sh .
RUN chmod +x start.sh && chown -R dotnet:dotnet /app

# Cambiar a usuario no-root
USER dotnet

# Configurar variables de entorno
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_HTTP_PORTS=8080

# Exponer puerto
EXPOSE 8080

# Comando de inicio
ENTRYPOINT ["./start.sh"]