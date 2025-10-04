# Dockerfile optimizado para Render.com
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar y restaurar dependencias
COPY *.csproj .
RUN dotnet restore

# Copiar código y publicar
COPY . .
RUN dotnet publish -c Release -o /app --no-restore

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

# Copiar archivos publicados
COPY --from=build /app .

# Variables de entorno para Render
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://0.0.0.0:$PORT
ENV DOTNET_RUNNING_IN_CONTAINER=true

# Exponer puerto dinámico
EXPOSE $PORT

# Comando de inicio
ENTRYPOINT ["dotnet", "parcial.dll"]