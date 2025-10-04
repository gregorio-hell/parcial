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

# Instalar SQLite para la base de datos
RUN apt-get update && apt-get install -y sqlite3 && rm -rf /var/lib/apt/lists/*

# Copiar archivos publicados
COPY --from=build /app/out .

# Crear directorio para la base de datos
RUN mkdir -p /app/data

# Configurar variables de entorno por defecto
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ConnectionStrings__DefaultConnection="DataSource=/app/data/app.db;Cache=Shared"

# Exponer puerto
EXPOSE 8080

# Dar permisos de escritura al directorio de datos
RUN chmod 777 /app/data

# Comando de inicio
ENTRYPOINT ["dotnet", "parcial.dll"]