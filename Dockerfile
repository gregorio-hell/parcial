# Usar la imagen base de .NET 9 SDK para build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar archivos de proyecto y restaurar dependencias
COPY *.csproj .
RUN dotnet restore

# Copiar todo el código fuente
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Usar la imagen runtime de .NET 9 para producción
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Instalar herramientas necesarias
RUN apt-get update && \
    apt-get install -y sqlite3 curl && \
    rm -rf /var/lib/apt/lists/*

# Copiar archivos publicados
COPY --from=build /app/publish .

# Crear directorio para base de datos con permisos amplios
RUN mkdir -p /tmp/data && chmod 777 /tmp/data

# Variables de entorno para Render
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:$PORT
ENV DOTNET_RUNNING_IN_CONTAINER=true
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=1

# Puerto por defecto
EXPOSE $PORT

# Comando de inicio simple
CMD ["dotnet", "parcial.dll"]